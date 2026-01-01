using Whisper.net;

namespace WhisperSample.Services.Whisper;

public sealed class WhisperTranscriptionService : IDisposable
{
    private const int WhisperSampleRate = 16000;

    private readonly WhisperModelService _modelService;
    private WhisperFactory? _factory;
    private string? _loadedModelPath;
    private WhisperProcessor? _processor;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public WhisperTranscriptionService(WhisperModelService modelService)
    {
        _modelService = modelService;
    }

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        var modelPath = await _modelService.GetModelPathAsync(ct);
        if (_factory != null && string.Equals(_loadedModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
            return;

        _factory?.Dispose();
        _factory = WhisperFactory.FromPath(modelPath);
        _loadedModelPath = modelPath;

        // モデルが変わったら processor も作り直す
        _processor?.Dispose();
        _processor = null;
    }

    public async Task<string> TranscribePcm16LeAsync(byte[] pcm16Le, int sampleRate, short channels, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await EnsureLoadedAsync(ct);

            var pcm16kMono = Ensure16kMonoPcm16Le(pcm16Le, sampleRate, channels);
            var samples = Pcm16LeToFloat(pcm16kMono);
            if (samples.Length < WhisperSampleRate)
                return string.Empty;

            _processor ??= _factory!.CreateBuilder()
                .WithLanguage("ja")
                .Build();

            var sb = new System.Text.StringBuilder();
            await foreach (var segment in _processor.ProcessAsync(samples, ct))
            {
                if (!string.IsNullOrWhiteSpace(segment.Text))
                    sb.Append(segment.Text);
            }

            return sb.ToString().Trim();
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _processor?.Dispose();
        _processor = null;
        _factory?.Dispose();
        _factory = null;

        _gate.Dispose();
    }

    private static float[] Pcm16LeToFloat(byte[] monoPcm16Le)
    {
        var samples = monoPcm16Le.Length / 2;
        if (samples <= 0)
            return Array.Empty<float>();

        var floats = new float[samples];
        const float inv = 1.0f / 32768.0f;
        for (var i = 0; i < samples; i++)
        {
            var b0 = monoPcm16Le[i * 2];
            var b1 = monoPcm16Le[i * 2 + 1];
            short s = (short)(b0 | (b1 << 8));
            floats[i] = s * inv;
        }
        return floats;
    }

    private static byte[] Ensure16kMonoPcm16Le(byte[] pcm16Le, int sampleRate, short channels)
    {
        if (channels <= 0)
            channels = 1;
        if (sampleRate <= 0)
            sampleRate = 16000;

        if (sampleRate == 16000 && channels == 1)
            return pcm16Le;

        var mono = channels == 1 ? pcm16Le : DownmixToMonoPcm16Le(pcm16Le, channels);
        if (sampleRate == 16000)
            return mono;

        return ResamplePcm16LeLinear(mono, sampleRate, 16000);
    }

    private static byte[] DownmixToMonoPcm16Le(byte[] interleavedPcm16Le, short channels)
    {
        var bytesPerFrame = channels * 2;
        if (bytesPerFrame <= 0 || interleavedPcm16Le.Length < bytesPerFrame)
            return Array.Empty<byte>();

        var frames = interleavedPcm16Le.Length / bytesPerFrame;
        var outBytes = new byte[frames * 2];

        for (var f = 0; f < frames; f++)
        {
            int sum = 0;
            for (var ch = 0; ch < channels; ch++)
            {
                var i = f * bytesPerFrame + ch * 2;
                short s = (short)(interleavedPcm16Le[i] | (interleavedPcm16Le[i + 1] << 8));
                sum += s;
            }

            short avg = (short)(sum / channels);
            outBytes[f * 2] = (byte)(avg & 0xFF);
            outBytes[f * 2 + 1] = (byte)((avg >> 8) & 0xFF);
        }

        return outBytes;
    }

    private static byte[] ResamplePcm16LeLinear(byte[] monoPcm16Le, int fromRate, int toRate)
    {
        if (fromRate <= 0 || toRate <= 0)
            return monoPcm16Le;
        if (fromRate == toRate)
            return monoPcm16Le;

        var inSamples = monoPcm16Le.Length / 2;
        if (inSamples <= 1)
            return Array.Empty<byte>();

        var outSamples = (int)Math.Round(inSamples * (double)toRate / fromRate);
        if (outSamples <= 1)
            return Array.Empty<byte>();

        short ReadSample(int idx)
        {
            var i = idx * 2;
            return (short)(monoPcm16Le[i] | (monoPcm16Le[i + 1] << 8));
        }

        var outBytes = new byte[outSamples * 2];
        for (var i = 0; i < outSamples; i++)
        {
            var pos = i * (double)(inSamples - 1) / (outSamples - 1);
            var left = (int)Math.Floor(pos);
            var right = Math.Min(left + 1, inSamples - 1);
            var t = pos - left;

            var s0 = ReadSample(left);
            var s1 = ReadSample(right);
            var s = (short)Math.Round(s0 + (s1 - s0) * t);

            outBytes[i * 2] = (byte)(s & 0xFF);
            outBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }

        return outBytes;
    }
}

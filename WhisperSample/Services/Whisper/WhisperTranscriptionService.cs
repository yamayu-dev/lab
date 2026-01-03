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

            // iOS側（AudioStreamSource）で 16kHz / mono / PCM16 に統一する前提
            if (sampleRate != WhisperSampleRate || channels != 1)
                return string.Empty;

            var samples = Pcm16LeToFloat(pcm16Le);
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
}

using WhisperSample.Services.Audio;
using WhisperSample.Services.Whisper;

namespace WhisperSample.Services;

public sealed class RealtimeTranscriber : IAsyncDisposable
{
    private readonly Func<IAudioStreamSource> _streamSourceFactory;
    private readonly WhisperTranscriptionService _whisper;

    private IAudioStreamSource? _source;

    /// <summary>
    /// RMS がこの閾値未満なら無音とみなして推論をスキップする。
    /// Whisper の hallucination（「ご視聴ありがとうございました」等）の前段防止。
    /// </summary>
    private const float SilenceRmsThreshold = 0.005f;

    public RealtimeTranscriber(Func<IAudioStreamSource> streamSourceFactory, WhisperTranscriptionService whisper)
    {
        _streamSourceFactory = streamSourceFactory;
        _whisper = whisper;
    }

    public async IAsyncEnumerable<string> RunAsync(
        TimeSpan interval,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        _source = _streamSourceFactory();

        // 16 kHz mono Float32 — サンプル数でカウント
        var buffer = new List<float>(capacity: 16000 * 20);

        void OnChunk(object? s, AudioChunkEventArgs e)
        {
            lock (buffer)
            {
                buffer.AddRange(e.Samples.AsSpan(0, e.Count).ToArray());

                // 最大 40 秒ぶんに制限
                const int maxSamples = 16000 * 40;
                if (buffer.Count > maxSamples)
                    buffer.RemoveRange(0, buffer.Count - maxSamples);
            }
        }

        _source.AudioChunkReady += OnChunk;

        try
        {
            await _source.StartAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(interval, ct);

                float[] snapshot;
                lock (buffer)
                {
                    snapshot = buffer.ToArray();
                }

                // 2 秒未満はスキップ
                if (snapshot.Length < 16000 * 2)
                    continue;

                // 無音（RMS が閾値未満）なら推論自体をスキップ
                if (CalculateRms(snapshot) < SilenceRmsThreshold)
                {
                    lock (buffer)
                        buffer.Clear();
                    continue;
                }

                var text = await _whisper.TranscribeAsync(snapshot, ct);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    yield return text;

                    lock (buffer)
                        buffer.Clear();
                }
            }
        }
        finally
        {
            _source.AudioChunkReady -= OnChunk;
            await _source.StopAsync(CancellationToken.None);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_source != null)
            await _source.DisposeAsync();

        _whisper.Dispose();
    }

    private static float CalculateRms(float[] samples)
    {
        if (samples.Length == 0) return 0f;
        double sum = 0;
        for (int i = 0; i < samples.Length; i++)
            sum += (double)samples[i] * samples[i];
        return (float)Math.Sqrt(sum / samples.Length);
    }
}

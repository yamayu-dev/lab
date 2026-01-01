using WhisperSample.Services.Audio;
using WhisperSample.Services.Whisper;

namespace WhisperSample.Services;

public sealed class RealtimeTranscriber : IAsyncDisposable
{
    private readonly Func<IAudioStreamSource> _streamSourceFactory;
    private readonly WhisperTranscriptionService _whisper;

    private IAudioStreamSource? _source;

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

        var pcmBuffer = new List<byte>(capacity: 16000 * 2 * 20);
        var inputSampleRate = 16000;
        short inputChannels = 1;

        void OnChunk(object? s, PcmChunkEventArgs e)
        {
            lock (pcmBuffer)
            {
                pcmBuffer.AddRange(e.Pcm16Le.AsSpan(0, e.Bytes).ToArray());
                inputSampleRate = e.SampleRate;
                inputChannels = e.Channels;

                // 最大40秒ぶん程度に制限
                const int maxBufferBytes = 16000 * 2 * 40;
                if (pcmBuffer.Count > maxBufferBytes)
                    pcmBuffer.RemoveRange(0, pcmBuffer.Count - maxBufferBytes);
            }
        }

        _source.PcmChunk += OnChunk;

        try
        {
            await _source.StartAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(interval, ct);

                byte[] snapshot;
                int sr;
                short ch;
                lock (pcmBuffer)
                {
                    snapshot = pcmBuffer.ToArray();
                    sr = inputSampleRate;
                    ch = inputChannels;
                }

                // 2秒未満はスキップ（最小実装なので雑に）
                if (snapshot.Length < 16000 * 2 * 2)
                    continue;

                var text = await _whisper.TranscribePcm16LeAsync(snapshot, sr, ch, ct);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    yield return text;

                    // 推論したらバッファをクリア（未確定/確定の判定なしでOKとのことなので最小に）
                    lock (pcmBuffer)
                        pcmBuffer.Clear();
                }
            }
        }
        finally
        {
            _source.PcmChunk -= OnChunk;
            await _source.StopAsync(CancellationToken.None);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_source != null)
            await _source.DisposeAsync();

        _whisper.Dispose();
    }
}

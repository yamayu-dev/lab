namespace WhisperSample.Services.Audio;

public interface IAudioStreamSource : IAsyncDisposable
{
    Task<bool> EnsurePermissionsAsync(CancellationToken ct = default);

    Task StartAsync(CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);

    event EventHandler<PcmChunkEventArgs>? PcmChunk;
}

public sealed class PcmChunkEventArgs : EventArgs
{
    public PcmChunkEventArgs(byte[] pcm16le, int bytes, int sampleRate, short channels)
    {
        Pcm16Le = pcm16le;
        Bytes = bytes;
        SampleRate = sampleRate;
        Channels = channels;
    }

    public byte[] Pcm16Le { get; }
    public int Bytes { get; }
    public int SampleRate { get; }
    public short Channels { get; }
}

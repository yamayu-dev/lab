namespace WhisperSample.Services.Audio;

// 非iOS向けスタブ（iOSでは `.ios.cs` 側の AudioStreamSource が使用される想定）
public sealed class UnsupportedAudioStreamSource : IAudioStreamSource
{
    public event EventHandler<PcmChunkEventArgs>? PcmChunk;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task<bool> EnsurePermissionsAsync(CancellationToken ct = default)
        => Task.FromResult(false);

    public Task StartAsync(CancellationToken ct = default)
        => throw new PlatformNotSupportedException("AudioStreamSource is supported on iOS only.");

    public Task StopAsync(CancellationToken ct = default)
        => throw new PlatformNotSupportedException("AudioStreamSource is supported on iOS only.");
}

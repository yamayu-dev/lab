namespace WhisperSample.Services.Audio;

public interface IAudioStreamSource : IAsyncDisposable
{
    Task<bool> EnsurePermissionsAsync(CancellationToken ct = default);

    Task StartAsync(CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// 16 kHz / mono / Float32 の正規化済み音声チャンクを通知する。
    /// </summary>
    event EventHandler<AudioChunkEventArgs>? AudioChunkReady;
}

/// <summary>
/// 16 kHz mono Float32 ([-1,1]) の音声チャンク。
/// Whisper.net の ProcessAsync(float[]) にそのまま渡せる形式。
/// </summary>
public sealed class AudioChunkEventArgs : EventArgs
{
    public AudioChunkEventArgs(float[] samples, int count)
    {
        Samples = samples;
        Count = count;
    }

    /// <summary>有効なサンプルを含むバッファ。Count 個まで有効。</summary>
    public float[] Samples { get; }

    /// <summary>Samples 内の有効サンプル数。</summary>
    public int Count { get; }
}

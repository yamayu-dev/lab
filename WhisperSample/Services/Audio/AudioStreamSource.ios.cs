using AVFoundation;
using Foundation;
using System.Runtime.InteropServices;

namespace WhisperSample.Services.Audio;

public sealed class AudioStreamSource : IAudioStreamSource
{
    private readonly AVAudioEngine _engine = new();
    private bool _started;
    private int _inputSampleRate;
    private short _inputChannels;

    public event EventHandler<PcmChunkEventArgs>? PcmChunk;

    public async Task<bool> EnsurePermissionsAsync(CancellationToken ct = default)
    {
        var session = AVAudioSession.SharedInstance();

        // すでに決まっているなら即返す
        if (session.RecordPermission == AVAudioSessionRecordPermission.Granted)
            return true;
        if (session.RecordPermission == AVAudioSessionRecordPermission.Denied)
            return false;

        // Undetermined の場合は、ユーザー操作で確定するまで待つ
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.RequestRecordPermission(granted => tcs.TrySetResult(granted));

        using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
        try
        {
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started)
            return;

        var ok = await EnsurePermissionsAsync(ct);
        if (!ok)
            throw new UnauthorizedAccessException("Microphone permission was not granted.");

        var session = AVAudioSession.SharedInstance();
        NSError? err;
        session.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker, out err);
        if (err != null) throw new NSErrorException(err);
        session.SetMode(AVAudioSessionMode.Default, out err);
        if (err != null) throw new NSErrorException(err);
        session.SetPreferredSampleRate(16000, out err);
        if (err != null) throw new NSErrorException(err);

        await Task.Delay(150, ct);

        session.SetActive(true, out err);
        if (err != null) throw new NSErrorException(err);

        var inputNode = _engine.InputNode;
        var format = inputNode.GetBusOutputFormat(0);

        _inputSampleRate = (int)Math.Round(format.SampleRate);
        _inputChannels = (short)format.ChannelCount;

        inputNode.RemoveTapOnBus(0);
        inputNode.InstallTapOnBus(0, 1024, format, (buffer, _) =>
        {
            try
            {
                var pcm16 = ExtractPcm16LeInterleaved(buffer);
                if (pcm16.Length > 0)
                    PcmChunk?.Invoke(this, new PcmChunkEventArgs(pcm16, pcm16.Length, _inputSampleRate, _inputChannels));
            }
            catch
            {
                // tapスレッド例外でエンジンが落ちるのを避ける
            }
        });

        _engine.Prepare();
        _engine.StartAndReturnError(out err);
        if (err != null) throw new NSErrorException(err);

        _started = true;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (!_started)
            return Task.CompletedTask;

        _engine.InputNode.RemoveTapOnBus(0);
        _engine.Stop();
        _started = false;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            _engine.InputNode.RemoveTapOnBus(0);
        }
        catch { }

        _engine.Stop();
        _engine.Dispose();
        return ValueTask.CompletedTask;
    }

    private static byte[] ExtractPcm16LeInterleaved(AVAudioPcmBuffer buffer)
    {
        var frames = (int)buffer.FrameLength;
        if (frames <= 0)
            return Array.Empty<byte>();

        var int16Data = buffer.Int16ChannelData;
        if (int16Data != IntPtr.Zero)
        {
            var data = new byte[frames * 2];
            Marshal.Copy(int16Data, data, 0, data.Length);
            return data;
        }

        var floatData = buffer.FloatChannelData;
        if (floatData == IntPtr.Zero)
            return Array.Empty<byte>();

        var ch0Ptr = Marshal.ReadIntPtr(floatData, 0);
        if (ch0Ptr == IntPtr.Zero)
            return Array.Empty<byte>();

        var floats = new float[frames];
        Marshal.Copy(ch0Ptr, floats, 0, frames);

        var outBytes = new byte[frames * 2];
        for (var i = 0; i < frames; i++)
        {
            var f = floats[i];
            if (f > 1f) f = 1f;
            if (f < -1f) f = -1f;
            var s = (short)Math.Round(f * short.MaxValue);
            outBytes[i * 2] = (byte)(s & 0xFF);
            outBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }
        return outBytes;
    }
}

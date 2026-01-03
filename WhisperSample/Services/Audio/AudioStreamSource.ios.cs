using AVFoundation;
using Foundation;
using System.Runtime.InteropServices;

namespace WhisperSample.Services.Audio;

public sealed class AudioStreamSource : IAudioStreamSource
{
    private readonly AVAudioEngine _engine = new();
    private bool _started;

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

        inputNode.RemoveTapOnBus(0);
        inputNode.InstallTapOnBus(0, 1024, format, (buffer, _) =>
        {
            try
            {
                // 入力を 16kHz mono PCM16 に統一してから発火
                var pcm16kMono = ConvertTo16kMonoPcm16Le(buffer);
                if (pcm16kMono.Length > 0)
                {
                    PcmChunk?.Invoke(this, new PcmChunkEventArgs(pcm16kMono, pcm16kMono.Length, 16000, 1));
                }
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
        finally
        {
            _engine.Stop();
            _engine.Dispose();
        }
        return ValueTask.CompletedTask;
    }

    private byte[] ConvertTo16kMonoPcm16Le(AVAudioPcmBuffer inBuffer)
    {
        if (inBuffer.FrameLength <= 0)
            return Array.Empty<byte>();

        var currentFormat = inBuffer.Format;

        if (Math.Abs(currentFormat.SampleRate - 16000.0) < 0.1 &&
            currentFormat.ChannelCount == 1 &&
            currentFormat.CommonFormat == AVAudioCommonFormat.PCMInt16)
        {
            return ExtractPcm16LeInterleaved(inBuffer);
        }

        AVAudioPcmBuffer currentBuffer = inBuffer;
        AVAudioPcmBuffer? ownedMonoBuffer = null;
        AVAudioPcmBuffer? ownedResampledBuffer = null;

        try
        {
            // Step 1: downmix to mono Float32 if needed.
            if (currentFormat.ChannelCount != 1 || currentFormat.CommonFormat != AVAudioCommonFormat.PCMFloat32)
            {
                var monoFormat = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, currentFormat.SampleRate, 1, false);
                using var downConverter = new AVAudioConverter(currentFormat, monoFormat);
                downConverter.PrimeMethod = AVAudioConverterPrimeMethod.None;
                var monoBuffer = new AVAudioPcmBuffer(monoFormat, currentBuffer.FrameLength);
                monoBuffer.FrameLength = 0;
                ownedMonoBuffer = monoBuffer;
                
                bool fed = false;
                downConverter.ConvertToBuffer(monoBuffer, out NSError? err, (uint _, out AVAudioConverterInputStatus status) =>
                {
                    if (fed)
                    {
                        status = AVAudioConverterInputStatus.NoDataNow;
                        return null;
                    }
                    fed = true;
                    status = AVAudioConverterInputStatus.HaveData;
                    return currentBuffer;
                });
                if (err != null)
                    throw new NSErrorException(err);
                
                if (monoBuffer.FrameLength == 0)
                    return Array.Empty<byte>();
                
                currentBuffer = monoBuffer;
                currentFormat = monoFormat;
            }

            // Step 2: resample to 16 kHz if needed.
            if (Math.Abs(currentFormat.SampleRate - 16000.0) > 0.1)
            {
                var resampleFormat = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, 16000, 1, false);
                using var resampler = new AVAudioConverter(currentFormat, resampleFormat);
                resampler.PrimeMethod = AVAudioConverterPrimeMethod.None;
                double ratio = 16000.0 / currentFormat.SampleRate;
                uint outCapacity = (uint)Math.Ceiling(currentBuffer.FrameLength * ratio) + 1;
                var resampledBuffer = new AVAudioPcmBuffer(resampleFormat, outCapacity);
                resampledBuffer.FrameLength = 0;
                ownedResampledBuffer = resampledBuffer;
                
                bool given = false;
                resampler.ConvertToBuffer(resampledBuffer, out NSError? err2, (uint _, out AVAudioConverterInputStatus status) =>
                {
                    if (given)
                    {
                        status = AVAudioConverterInputStatus.NoDataNow;
                        return null;
                    }
                    given = true;
                    status = AVAudioConverterInputStatus.HaveData;
                    return currentBuffer;
                });
                if (err2 != null)
                    throw new NSErrorException(err2);
                
                if (resampledBuffer.FrameLength == 0)
                    return Array.Empty<byte>();
                
                currentBuffer = resampledBuffer;
                currentFormat = resampleFormat;
            }

            // Now currentBuffer is mono and 16 kHz, possibly Float32.
            if (currentFormat.CommonFormat == AVAudioCommonFormat.PCMInt16)
            {
                return ExtractPcm16LeInterleaved(currentBuffer);
            }

            if (currentFormat.CommonFormat == AVAudioCommonFormat.PCMFloat32)
            {
                int frames = (int)currentBuffer.FrameLength;
                var floatPtr = currentBuffer.FloatChannelData;
                if (floatPtr == IntPtr.Zero)
                    return Array.Empty<byte>();
                var ch0Ptr = Marshal.ReadIntPtr(floatPtr, 0);
                if (ch0Ptr == IntPtr.Zero)
                    return Array.Empty<byte>();

                // WIP: リングバッファやunsafeを検討
                var floats = new float[frames];
                Marshal.Copy(ch0Ptr, floats, 0, frames);
                var outBytes = new byte[frames * 2];
                for (int i = 0; i < frames; i++)
                {
                    var f = floats[i];
                    if (f > 1f) f = 1f;
                    if (f < -1f) f = -1f;
                    short s = (short)Math.Round(f * short.MaxValue);
                    outBytes[i * 2] = (byte)(s & 0xFF);
                    outBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
                }
                return outBytes;
            }

            // As a fallback, extract whatever PCM16LE can be obtained.
            return ExtractPcm16LeInterleaved(currentBuffer);
        }
        finally
        {
            ownedMonoBuffer?.Dispose();
            ownedResampledBuffer?.Dispose();
        }
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

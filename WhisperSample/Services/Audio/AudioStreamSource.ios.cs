using AVFoundation;
using Foundation;
using System.Buffers;
using System.Runtime.InteropServices;

namespace WhisperSample.Services.Audio;

public sealed class AudioStreamSource : IAudioStreamSource
{
    private readonly AVAudioEngine _engine = new();
    private bool _started;

    // AVAudioConverter はフォーマットが変わらない限り再利用する
    private AVAudioFormat? _cachedInputFormat;
    private AVAudioConverter? _downmixConverter;   // input → mono Float32 (same rate)
    private AVAudioConverter? _resampleConverter;   // mono Float32 → mono Float32 16kHz

    private static readonly AVAudioFormat TargetFormat =
        new(AVAudioCommonFormat.PCMFloat32, 16000, 1, false);

    public event EventHandler<AudioChunkEventArgs>? AudioChunkReady;

    public async Task<bool> EnsurePermissionsAsync(CancellationToken ct = default)
    {
        var session = AVAudioSession.SharedInstance();

        if (session.RecordPermission == AVAudioSessionRecordPermission.Granted)
            return true;
        if (session.RecordPermission == AVAudioSessionRecordPermission.Denied)
            return false;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.RequestRecordPermission(granted => tcs.TrySetResult(granted));

        using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
        try { return await tcs.Task; }
        catch (OperationCanceledException) { return false; }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started) return;

        if (!await EnsurePermissionsAsync(ct))
            throw new UnauthorizedAccessException("Microphone permission was not granted.");

        var session = AVAudioSession.SharedInstance();
        NSError? err;
        session.SetCategory(AVAudioSessionCategory.PlayAndRecord,
                            AVAudioSessionCategoryOptions.DefaultToSpeaker, out err);
        if (err != null) throw new NSErrorException(err);
        session.SetMode(AVAudioSessionMode.Default, out err);
        if (err != null) throw new NSErrorException(err);
        session.SetPreferredSampleRate(16000, out err);
        if (err != null) throw new NSErrorException(err);

        await Task.Delay(150, ct);

        session.SetActive(true, out err);
        if (err != null) throw new NSErrorException(err);

        var inputNode = _engine.InputNode;
        var inputFormat = inputNode.GetBusOutputFormat(0);

        // tap 開始前に converter を準備
        EnsureConverters(inputFormat);

        inputNode.RemoveTapOnBus(0);
        inputNode.InstallTapOnBus(0, 4096, inputFormat, (buffer, _) =>
        {
            try
            {
                var floats = ConvertToMono16kFloat(buffer);
                if (floats != null && floats.Length > 0)
                    AudioChunkReady?.Invoke(this, new AudioChunkEventArgs(floats, floats.Length));
            }
            catch
            {
                // tap スレッド例外でエンジンが落ちるのを避ける
            }
        });

        _engine.Prepare();
        _engine.StartAndReturnError(out err);
        if (err != null) throw new NSErrorException(err);

        _started = true;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (!_started) return Task.CompletedTask;

        _engine.InputNode.RemoveTapOnBus(0);
        _engine.Stop();
        _started = false;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        try { _engine.InputNode.RemoveTapOnBus(0); } catch { }
        _engine.Stop();
        _engine.Dispose();
        DisposeConverters();
        return ValueTask.CompletedTask;
    }

    // ── 変換パイプライン ──────────────────────────────

    /// <summary>
    /// 入力 AVAudioPcmBuffer → 16 kHz / mono / Float32 の float[] を返す。
    /// </summary>
    private float[]? ConvertToMono16kFloat(AVAudioPcmBuffer inBuffer)
    {
        if (inBuffer.FrameLength <= 0) return null;

        var fmt = inBuffer.Format;
        EnsureConverters(fmt);

        AVAudioPcmBuffer current = inBuffer;
        AVAudioPcmBuffer? ownedMono = null;
        AVAudioPcmBuffer? ownedResampled = null;

        try
        {
            // Step 1: mono Float32 化（必要なとき）
            if (fmt.ChannelCount != 1 || fmt.CommonFormat != AVAudioCommonFormat.PCMFloat32)
            {
                var monoFmt = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, fmt.SampleRate, 1, false);
                ownedMono = new AVAudioPcmBuffer(monoFmt, current.FrameLength);
                ownedMono.FrameLength = 0;

                if (!Convert(_downmixConverter!, current, ownedMono))
                    return null;

                current = ownedMono;
                fmt = monoFmt;
            }

            // Step 2: 16 kHz リサンプル（必要なとき）
            if (Math.Abs(fmt.SampleRate - 16000.0) > 0.1)
            {
                double ratio = 16000.0 / fmt.SampleRate;
                uint capacity = (uint)Math.Ceiling(current.FrameLength * ratio) + 1;
                ownedResampled = new AVAudioPcmBuffer(TargetFormat, capacity);
                ownedResampled.FrameLength = 0;

                if (!Convert(_resampleConverter!, current, ownedResampled))
                    return null;

                current = ownedResampled;
            }

            // Step 3: Float32 バッファからマネージド float[] へコピー
            return ExtractFloat32(current);
        }
        finally
        {
            ownedMono?.Dispose();
            ownedResampled?.Dispose();
        }
    }

    /// <summary>AVAudioConverter で src→dst を変換する。成功なら true。</summary>
    private static bool Convert(AVAudioConverter converter, AVAudioPcmBuffer src, AVAudioPcmBuffer dst)
    {
        bool fed = false;
        converter.ConvertToBuffer(dst, out NSError? err, (uint _, out AVAudioConverterInputStatus status) =>
        {
            if (fed) { status = AVAudioConverterInputStatus.NoDataNow; return null; }
            fed = true;
            status = AVAudioConverterInputStatus.HaveData;
            return src;
        });
        return err == null && dst.FrameLength > 0;
    }

    /// <summary>mono Float32 の AVAudioPcmBuffer から float[] を取り出す。</summary>
    private static float[]? ExtractFloat32(AVAudioPcmBuffer buffer)
    {
        int frames = (int)buffer.FrameLength;
        if (frames <= 0) return null;

        var chDataPtr = buffer.FloatChannelData;
        if (chDataPtr == IntPtr.Zero) return null;

        var ch0 = Marshal.ReadIntPtr(chDataPtr, 0);
        if (ch0 == IntPtr.Zero) return null;

        var result = new float[frames];
        Marshal.Copy(ch0, result, 0, frames);
        return result;
    }

    // ── Converter キャッシュ ───────────────────────────

    private void EnsureConverters(AVAudioFormat inputFormat)
    {
        var cached = _cachedInputFormat;
        if (!ReferenceEquals(cached, null) &&
            Math.Abs(cached.SampleRate - inputFormat.SampleRate) < 0.1 &&
            cached.ChannelCount == inputFormat.ChannelCount &&
            cached.CommonFormat == inputFormat.CommonFormat)
        {
            return; // フォーマット変更なし
        }

        DisposeConverters();
        _cachedInputFormat = inputFormat;

        var monoFloat = new AVAudioFormat(AVAudioCommonFormat.PCMFloat32, inputFormat.SampleRate, 1, false);

        _downmixConverter = new AVAudioConverter(inputFormat, monoFloat)
            { PrimeMethod = AVAudioConverterPrimeMethod.None };

        _resampleConverter = new AVAudioConverter(monoFloat, TargetFormat)
            { PrimeMethod = AVAudioConverterPrimeMethod.None };
    }

    private void DisposeConverters()
    {
        _downmixConverter?.Dispose();  _downmixConverter = null;
        _resampleConverter?.Dispose(); _resampleConverter = null;
        _cachedInputFormat = null;
    }
}

using Whisper.net;

namespace WhisperSample.Services.Whisper;

public sealed class WhisperTranscriptionService : IDisposable
{
    private const int WhisperSampleRate = 16000;

    /// <summary>
    /// NoSpeechProbability がこの閾値以上のセグメントは無音と見なしてスキップする。
    /// Whisper の hallucination（「ご視聴ありがとうございました」等）対策。
    /// </summary>
    private const float NoSpeechThreshold = 0.6f;

    private readonly WhisperModelService _modelService;
    private WhisperFactory? _factory;
    private string? _loadedModelPath;
    private WhisperProcessor? _processor;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public WhisperTranscriptionService(WhisperModelService modelService)
    {
        _modelService = modelService;
    }

    /// <summary>
    /// 16 kHz / mono / Float32 のサンプル配列をそのまま Whisper に渡して文字起こしする。
    /// </summary>
    public async Task<string> TranscribeAsync(float[] samples, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await EnsureLoadedAsync(ct);

            // 1 秒未満のデータはノイズなのでスキップ
            if (samples.Length < WhisperSampleRate)
                return string.Empty;

            _processor ??= _factory!.CreateBuilder()
                .WithLanguage("ja")
                .Build();

            var sb = new System.Text.StringBuilder();
            await foreach (var segment in _processor.ProcessAsync(samples, ct))
            {
                // 無音区間の hallucination を除外
                if (segment.NoSpeechProbability >= NoSpeechThreshold)
                    continue;

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

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        var modelPath = await _modelService.GetModelPathAsync(ct);
        if (_factory != null && string.Equals(_loadedModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
            return;

        _factory?.Dispose();
        _factory = WhisperFactory.FromPath(modelPath);
        _loadedModelPath = modelPath;

        _processor?.Dispose();
        _processor = null;
    }
}

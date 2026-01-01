namespace WhisperSample.Services.Whisper;

public sealed class WhisperModelService
{
    private const string ModelAssetPath = "Models/ggml-small.bin";
    private string? _cachedModelPath;

    public async Task<string> GetModelDiagnosticsAsync(CancellationToken ct = default)
    {
        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            var targetPath = Path.Combine(cacheDir, "ggml-small.bin");

            var packagedExists = await FileSystem.AppPackageFileExistsAsync(ModelAssetPath);
            long packagedSize = -1;
            if (packagedExists)
            {
                await using var s = await FileSystem.OpenAppPackageFileAsync(ModelAssetPath);
                packagedSize = s.Length;
            }

            var cachedExists = File.Exists(targetPath);
            long cachedSize = -1;
            if (cachedExists)
                cachedSize = new FileInfo(targetPath).Length;

            return $"ModelAsset='{ModelAssetPath}' packagedExists={packagedExists} packagedSize={packagedSize} cachedPath='{targetPath}' cachedExists={cachedExists} cachedSize={cachedSize}";
        }
        catch (Exception ex)
        {
            return "ModelDiagnostics[Error] " + ex.Message;
        }
    }

    public async Task<string> GetModelPathAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(_cachedModelPath) && File.Exists(_cachedModelPath))
            return _cachedModelPath;

        var cacheDir = FileSystem.CacheDirectory;
        var targetPath = Path.Combine(cacheDir, "ggml-small.bin");

        if (!File.Exists(targetPath))
        {
            // パッケージ内にモデルが無いと FileNotFoundException が起きるので、まず存在をチェックして案内を出す
            var packagedExists = await FileSystem.AppPackageFileExistsAsync(ModelAssetPath);
            if (!packagedExists)
                throw new FileNotFoundException(
                    $"Whisper model not found in app package: '{ModelAssetPath}'. Place the file at 'WhisperSample/Models/ggml-small.bin' and ensure it's included as a MauiAsset.");

            await using var modelStream = await FileSystem.OpenAppPackageFileAsync(ModelAssetPath);
            await using var fs = File.Create(targetPath);
            await modelStream.CopyToAsync(fs, ct);
        }

        // 0バイトなどを早期に検出して、ネイティブ側で落ちるのを避ける
        var len = new FileInfo(targetPath).Length;
        if (len < 1024 * 1024)
            throw new InvalidDataException($"Whisper model looks too small ({len} bytes). Copy may have failed: '{targetPath}'");

        _cachedModelPath = targetPath;
        return targetPath;
    }
}

using Foundation;

namespace MauiSpeechAnalyzer.Services;

public interface ISpeechRecognitionService
{
    Task<bool> CheckPermissionsAsync();
    void StartRecognition(string localeIdentifier, Action<string, string, string> onResult, Action<string> onError);
    void StopRecognition();
    Task<int> CheckAssetStatusAsync();
    Task DownloadAssetsAsync(Action<double> onProgress);
}

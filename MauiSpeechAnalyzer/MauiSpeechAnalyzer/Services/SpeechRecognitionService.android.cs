#if ANDROID
namespace MauiSpeechAnalyzer.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    public Task<bool> CheckPermissionsAsync()
    {
        throw new PlatformNotSupportedException("Android is not supported yet.");
    }

    public void StartRecognition(string localeIdentifier, Action<string, string, string> onResult, Action<string> onError)
    {
        throw new PlatformNotSupportedException("Android is not supported yet.");
    }

    public void StopRecognition()
    {
        throw new PlatformNotSupportedException("Android is not supported yet.");
    }

    public Task<int> CheckAssetStatusAsync()
    {
        throw new PlatformNotSupportedException("Android is not supported yet.");
    }

    public Task DownloadAssetsAsync(Action<double> onProgress)
    {
        throw new PlatformNotSupportedException("Android is not supported yet.");
    }
}
#endif

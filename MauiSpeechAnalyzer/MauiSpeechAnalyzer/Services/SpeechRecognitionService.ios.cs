#if IOS || MACCATALYST
using SpeechAnalyzerKit;
using AVFoundation;
using Speech;

namespace MauiSpeechAnalyzer.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private SpeechAnalyzerWrapper? _wrapper;

    public async Task<bool> CheckPermissionsAsync()
    {
        // マイク権限チェック
        var micStatus = AVAudioSession.SharedInstance().RecordPermission;
        if (micStatus == AVAudioSessionRecordPermission.Denied)
            return false;

        if (micStatus == AVAudioSessionRecordPermission.Undetermined)
        {
            var tcs = new TaskCompletionSource<bool>();
            AVAudioSession.SharedInstance().RequestRecordPermission(granted => tcs.TrySetResult(granted));
            var micGranted = await tcs.Task;
            if (!micGranted)
                return false;
        }

        // 音声認識権限チェック
        var speechStatus = SFSpeechRecognizer.AuthorizationStatus;
        if (speechStatus == SFSpeechRecognizerAuthorizationStatus.Denied || 
            speechStatus == SFSpeechRecognizerAuthorizationStatus.Restricted)
            return false;

        if (speechStatus == SFSpeechRecognizerAuthorizationStatus.NotDetermined)
        {
            var tcs = new TaskCompletionSource<bool>();
            SFSpeechRecognizer.RequestAuthorization(status => 
            {
                tcs.TrySetResult(status == SFSpeechRecognizerAuthorizationStatus.Authorized);
            });
            var speechGranted = await tcs.Task;
            if (!speechGranted)
                return false;
        }

        return true;
    }

    public void StartRecognition(
        string localeIdentifier, 
        Action<string, string, string> onResult, 
        Action<string> onError)
    {
        _wrapper = new SpeechAnalyzerWrapper();
        
        _wrapper.StartRealtimeWithLocaleIdentifier(
            localeIdentifier,
            "",
            snapshot =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    onResult(snapshot.ConfirmedText, snapshot.PendingText, snapshot.CombinedText);
                });
            },
            error =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    onError(error);
                });
            }
        );
    }

    public void StopRecognition()
    {
        _wrapper?.Stop();
        _wrapper = null;
    }

    public async Task<int> CheckAssetStatusAsync()
    {
        var tcs = new TaskCompletionSource<int>();
        
        SpeechAnalyzerWrapper.CheckAssetStatusForJapanese(
            1, // progressiveTranscription
            status =>
            {
                tcs.TrySetResult((int)status);
            }
        );

        return await tcs.Task;
    }

    public async Task DownloadAssetsAsync(Action<double> onProgress)
    {
        var tcs = new TaskCompletionSource<bool>();

        SpeechAnalyzerWrapper.DownloadAssetsForJapanese(
            1, // progressiveTranscription
            progress =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    onProgress(progress);
                });
            },
            error =>
            {
                if (error == null)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    tcs.TrySetException(new Exception(error.ToString()));
                }
            }
        );

        await tcs.Task;
    }
}
#endif

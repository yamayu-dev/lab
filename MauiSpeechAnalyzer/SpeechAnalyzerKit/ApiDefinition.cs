using System;
using Foundation;
using ObjCRuntime;

namespace SpeechAnalyzerKit
{
    // TranscriptSnapshot の定義
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface TranscriptSnapshot
    {
        [Export("confirmedText")]
        string ConfirmedText { get; }

        [Export("pendingText")]
        string PendingText { get; }

        [Export("combinedText")]
        string CombinedText { get; }

        [Export("initWithConfirmedText:pendingText:combinedText:")]
        NativeHandle Constructor(string confirmedText, string pendingText, string combinedText);
    }

    // SpeechAnalyzerWrapper の定義
    [BaseType(typeof(NSObject))]
    interface SpeechAnalyzerWrapper
    {
        [Export("startRealtimeWithLocaleIdentifier:prefix:onResult:onError:")]
        void StartRealtimeWithLocaleIdentifier(
            string localeIdentifier,
            string prefix,
            Action<TranscriptSnapshot> onResult,
            Action<string> onError
        );

        [Export("stop")]
        void Stop();

        [Export("transcribeFileAtPath:localeIdentifier:prefix:onResult:onFormatInfo:onComplete:onError:")]
        void TranscribeFileAtPath(
            string path,
            string localeIdentifier,
            string prefix,
            Action<TranscriptSnapshot> onResult,
            Action<string> onFormatInfo,
            Action<string> onComplete,
            Action<string> onError
        );

        [Export("formatInfo")]
        string FormatInfo { get; }

        [Static]
        [Export("checkAssetStatusForJapaneseWithPreset:completion:")]
        void CheckAssetStatusForJapanese(nint preset, Action<nint> completion);

        [Static]
        [Export("downloadAssetsForJapaneseWithPreset:onProgress:onError:")]
        void DownloadAssetsForJapanese(
            nint preset,
            Action<double> onProgress,
            Action<NSString> onError
        );
    }
}


# SpeechAnalyzerKit

iOS 26.0以降のSpeechAnalyzer APIを使った音声認識機能を提供するフレームワークです。MAUI（.NET Multi-platform App UI）などのクロスプラットフォーム開発で利用できるようにxcframeworkとしてビルドされます。

## 機能

- **リアルタイム音声認識**: マイクからの音声をリアルタイムで文字起こし
- **ファイルからの文字起こし**: 音声ファイル（m4a、wavなど）を文字起こし
- **音声録音**: マイクからの音声を録音してファイルに保存
- **アセット管理**: SpeechAnalyzerに必要な言語モデルのダウンロードと管理

## 要件

- iOS 26.0以降（SpeechAnalyzer APIを使用）
- Xcode 16.0以降
- Swift 6.0以降

## ビルド方法

### xcframeworkのビルド

```bash
# Debug版とRelease版の両方をビルド
./build.sh

# Debug版のみビルド
./build.sh -d

# Release版のみビルド
./build.sh -r
```

ビルド成果物は以下の場所に生成されます：

- `build/debug/SpeechAnalyzerKit.xcframework` - Debug版
- `build/release/SpeechAnalyzerKit.xcframework` - Release版
- `framework/SpeechAnalyzerKit-Debug.xcframework.zip` - Debug版ZIP
- `framework/SpeechAnalyzerKit-Release.xcframework.zip` - Release版ZIP

## 使い方

### 1. 許可の取得

```swift
import SpeechAnalyzerKit

// 音声認識の許可をリクエスト
SpeechAnalyzerKitHelper.requestAuthorization { status in
    if status == .authorized {
        print("音声認識が許可されました")
    }
}

// 録音の許可をリクエスト（iOS 17.0以降）
if #available(iOS 17.0, *) {
    SpeechAnalyzerKitHelper.requestRecordPermission { granted in
        if granted {
            print("録音が許可されました")
        }
    }
}
```

### 2. リアルタイム音声認識

```swift
@MainActor
func startTranscription() {
    if #available(iOS 26.0, *) {
        let engine = SpeechAnalyzerKitHelper.createEngine();
        
        engine.startRealtime(
            locale: Locale(identifier: "ja-JP"),
            prefix: "",
            onResult: { snapshot in
                print("確定テキスト: \(snapshot.confirmedText)")
                print("暫定テキスト: \(snapshot.pendingText)")
            },
            onError: { error in
                print("エラー: \(error)")
            }
        )
    }
}
```

### 3. ファイルからの文字起こし

```swift
@MainActor
func transcribeFile(url: URL) async {
    if #available(iOS 26.0, *) {
        let engine = SpeechAnalyzerKitHelper.createEngine();
        
        await engine.transcribeFile(
            url: url,
            locale: Locale(identifier: "ja-JP"),
            prefix: "",
            onResult: { snapshot in
                print("文字起こし結果: \(snapshot.combinedText)")
            },
            onFormatInfo: { info in
                print("音声フォーマット: \(info)")
            },
            onComplete: { message in
                print("完了: \(message)")
            },
            onError: { error in
                print("エラー: \(error)")
            }
        )
    }
}
```

### 4. 音声録音

```swift
@MainActor
func recordAudio() {
    let recorder = SpeechAnalyzerKitHelper.createAudioRecorder();
    
    // 録音開始
    try? recorder.startRecording()
    
    // 録音停止
    recorder.stopRecording()
    
    // 録音したファイルのURL
    if let url = recorder.audioFileURL {
        print("録音ファイル: \(url)")
    }
}
```

## MAUI での利用

MAUI（.NET）プロジェクトでは、以下の手順で利用できます：

### 1. xcframeworkの配置

ビルドしたxcframeworkをMAUIプロジェクトの`Platforms/iOS/`ディレクトリに配置

### 2. プロジェクトファイルの設定

`.csproj`ファイルで参照を追加：

```xml
<ItemGroup>
  <NativeReference Include="Platforms\iOS\SpeechAnalyzerKit.xcframework">
    <Kind>Framework</Kind>
    <ForceLoad>True</ForceLoad>
  </NativeReference>
</ItemGroup>
```

### 3. C#バインディングの作成

`Platforms/iOS/SpeechAnalyzerBinding.cs`を作成：

```csharp
using Foundation;
using ObjCRuntime;
using UIKit;

namespace YourApp.iOS
{
    // TranscriptSnapshot
    [BaseType(typeof(NSObject))]
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

    // AudioRecorder
    [BaseType(typeof(NSObject))]
    interface AudioRecorder
    {
        [Export("audioFileURL")]
        NSUrl AudioFileURL { get; set; }

        [Export("audioDisplayName")]
        string AudioDisplayName { get; set; }

        [Export("isRecording")]
        bool IsRecording { get; }

        [Export("init")]
        NativeHandle Constructor();

        [Export("startRecording")]
        void StartRecording();

        [Export("stopRecording")]
        void StopRecording();

        [Export("deleteAudio")]
        void DeleteAudio();

        [Export("setAudioFrom:")]
        void SetAudio(NSUrl url);

        [Export("suggestedExportFileName")]
        string SuggestedExportFileName { get; }
    }

    // SpeechAnalyzerWrapper (iOS 26.0+)
    [iOS(26, 0)]
    [BaseType(typeof(NSObject))]
    interface SpeechAnalyzerWrapper
    {
        [Export("init")]
        NativeHandle Constructor();

        [Export("startRealtimeWithLocaleIdentifier:prefix:onResult:onError:")]
        void StartRealtime(string localeIdentifier, string prefix, 
            Action<TranscriptSnapshot> onResult, Action<string> onError);

        [Export("stop")]
        void Stop();

        [Export("transcribeFileAtPath:localeIdentifier:prefix:onResult:onFormatInfo:onComplete:onError:")]
        void TranscribeFile(string path, string localeIdentifier, string prefix,
            Action<TranscriptSnapshot> onResult, 
            Action<string> onFormatInfo,
            Action<string> onComplete, 
            Action<string> onError);

        [Export("formatInfo")]
        string FormatInfo { get; }

        [Static]
        [Export("checkAssetStatusForJapanese:completion:")]
        void CheckAssetStatusForJapanese(nint preset, Action<nint> completion);

        [Static]
        [Export("downloadAssetsForJapanese:onProgress:onError:")]
        void DownloadAssetsForJapanese(nint preset, 
            Action<double> onProgress, Action<string> onError);
    }

    // SpeechAnalyzerKitHelper
    [BaseType(typeof(NSObject))]
    interface SpeechAnalyzerKitHelper
    {
        [Static]
        [Export("requestAuthorization:")]
        void RequestAuthorization(Action<nint> completion);

        [Static]
        [Export("requestRecordPermission:")]
        void RequestRecordPermission(Action<bool> completion);

        [Static]
        [Export("createAudioRecorder")]
        AudioRecorder CreateAudioRecorder();

        [Static]
        [Export("availableLocales")]
        NSLocale[] AvailableLocales();

        [Static]
        [Export("isLocaleSupported:")]
        bool IsLocaleSupported(NSLocale locale);
    }
}
```

### 4. C#からの使用例

```csharp
using YourApp.iOS;

public class SpeechService
{
    private SpeechAnalyzerWrapper? wrapper;

    public async Task StartTranscription()
    {
        // 許可をリクエスト
        SpeechAnalyzerKitHelper.RequestAuthorization(status =>
        {
            if (status == 3) // Authorized
            {
                MainThread.BeginInvokeOnMainThread(StartRecognition);
            }
        });
    }

    private void StartRecognition()
    {
        if (OperatingSystem.IsIOSVersionAtLeast(26, 0))
        {
            wrapper = new SpeechAnalyzerWrapper();
            wrapper.StartRealtime(
                "ja-JP",
                "",
                snapshot =>
                {
                    Console.WriteLine($"認識結果: {snapshot.CombinedText}");
                },
                error =>
                {
                    Console.WriteLine($"エラー: {error}");
                }
            );
        }
    }

    public void StopTranscription()
    {
        wrapper?.Stop();
    }

    // 音声録音
    public void RecordAudio()
    {
        var recorder = SpeechAnalyzerKitHelper.CreateAudioRecorder();
        recorder.StartRecording();
        
        // 停止
        recorder.StopRecording();
        
        // ファイルパスを取得
        var url = recorder.AudioFileURL;
    }
}
```

## アーキテクチャ

- **SpeechAnalyzerKit**: メインインターフェース（許可管理、ファクトリメソッド）
- **SpeechAnalyzerEngine**: 音声認識エンジン（リアルタイム認識、ファイル認識）
- **AudioRecorder**: 音声録音機能
- **TranscriptSnapshot**: 認識結果のスナップショット

## ライセンス

プロジェクトに応じて適切なライセンスを設定してください。

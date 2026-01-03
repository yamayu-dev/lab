# SpeechAnalyzerKit Binding

このディレクトリには、SpeechAnalyzerKit Swift フレームワークの .NET iOS バインディングが含まれています。

## 構成

- `ApiDefinition.cs` - Objective-C/Swift API のバインディング定義
- `StructsAndEnums.cs` - 構造体と列挙型の定義
- `SpeechAnalyzerKit.csproj` - バインディングプロジェクト
- `extract_framework.ps1` - xcframework 展開スクリプト
- `Native/` - ネイティブ xcframework ファイル（git管理外）

## セットアップ

### 1. xcframework の展開

SpeechAnalyzerKit フレームワークの Zip を `Native/` ディレクトリに展開します。

```powershell
# Debug 版のみ展開
.\extract_framework.ps1 -Debug

# Release 版のみ展開
.\extract_framework.ps1 -Release

# 両方展開（デフォルト）
.\extract_framework.ps1
```

スクリプトは相対パスで動作し、以下の場所から Zip を探します：
- ソース: `..\..\SpeechAnalyzerKit\framework\*.xcframework.zip`
- 展開先: `.\Native\`

### 2. ビルド

```powershell
dotnet build
```

## バインディング API

### TranscriptSnapshot

音声認識結果のスナップショット

```csharp
public class TranscriptSnapshot
{
    public string ConfirmedText { get; }  // 確定テキスト
    public string PendingText { get; }    // 暫定テキスト
    public string CombinedText { get; }   // 結合テキスト
}
```

### SpeechAnalyzerWrapper

音声認識エンジンのラッパークラス

```csharp
public class SpeechAnalyzerWrapper
{
    // リアルタイム音声認識を開始
    public void StartRealtimeWithLocaleIdentifier(
        string localeIdentifier,
        string prefix,
        Action<TranscriptSnapshot> onResult,
        Action<string> onError
    );

    // 停止
    public void Stop();

    // ファイルから文字起こし
    public void TranscribeFileAtPath(
        string path,
        string localeIdentifier,
        string prefix,
        Action<TranscriptSnapshot> onResult,
        Action<string> onFormatInfo,
        Action<string> onComplete,
        Action<string> onError
    );

    // フォーマット情報
    public string FormatInfo { get; }

    // 静的メソッド
    public static void CheckAssetStatusForJapanese(nint preset, Action<nint> completion);
    public static void DownloadAssetsForJapanese(nint preset, Action<double> onProgress, Action<NSString> onError);
}
```

## 使用例

```csharp
using SpeechAnalyzerKit;

// インスタンス作成
var wrapper = new SpeechAnalyzerWrapper();

// リアルタイム音声認識開始
wrapper.StartRealtimeWithLocaleIdentifier(
    "ja-JP",
    "",
    result => {
        Console.WriteLine($"確定: {result.ConfirmedText}");
        Console.WriteLine($"暫定: {result.PendingText}");
    },
    error => {
        Console.WriteLine($"エラー: {error}");
    }
);

// 停止
wrapper.Stop();
```

## トラブルシューティング

### ビルドエラー: xcframework が見つからない

`extract_framework.ps1` を実行して xcframework を展開してください。

### バインディングエラー

`ApiDefinition.cs` のエクスポート属性が Swift の実際の API と一致していることを確認してください。

## 参考

- MyHelloKit/PocMauiApp の構成を参考にしています
- [.NET iOS Binding Documentation](https://learn.microsoft.com/ja-jp/xamarin/ios/platform/binding-objective-c/)

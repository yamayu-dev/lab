# WhisperSample — iOS リアルタイム文字起こし

.NET MAUI + [Whisper.net](https://github.com/sandrohanea/whisper.net) で、iPhone のマイク入力をリアルタイムに文字起こしする **最小実装** です。

- MVVM なし（コードビハインド直結）
- VAD・差分マージ等なし
- Float32 直通パイプライン（PCM16 変換を介さない）

---

## アーキテクチャ

```
┌──────────────────────┐
│    AVAudioEngine      │  マイク入力（48 kHz / stereo 等、デバイス依存）
│      + Tap            │
└──────────┬───────────┘
           │ AVAudioPcmBuffer
           ▼
┌──────────────────────────────┐
│  AudioStreamSource (.ios.cs) │  AVAudioConverter（キャッシュ済み）で
│  ① downmix → mono Float32   │    ダウンミックス + リサンプル
│  ② resample → 16 kHz        │  Marshal.Copy で float[] を取得
│  ③ float[] を発火            │
└──────────┬───────────────────┘
           │ AudioChunkReady(float[])
           ▼
┌──────────────────────────────┐
│  RealtimeTranscriber         │  List<float> に蓄積
│  5 秒間隔で snapshot →推論   │  最大 40 秒 / 最小 2 秒
└──────────┬───────────────────┘
           │ float[]
           ▼
┌──────────────────────────────┐
│  WhisperTranscriptionService │  ProcessAsync(float[]) で
│  Whisper.net (ggml C++)      │  Whisper C++ に直接渡す
└──────────────────────────────┘
```

### データ形式の流れ

| 地点 | 形式 | 備考 |
|---|---|---|
| マイク Tap | デバイスネイティブ（例: 48 kHz / stereo / Float32） | `AVAudioEngine.InputNode` の出力フォーマット |
| AudioStreamSource 出力 | **16 kHz / mono / Float32** | `AVAudioConverter` でダウンミックス＋リサンプル |
| RealtimeTranscriber バッファ | `List<float>` | サンプル数ベースで管理 |
| Whisper.net 入力 | `float[]` → C++ 側で `fixed(float*)` | 変換なしでそのまま渡る |

> **設計ポイント**: マイクから Whisper まで一貫して Float32 を維持し、PCM16 への変換・逆変換を排除。精度劣化とCPU負荷の両方を回避しています。

---

## プロジェクト構成

```
WhisperSample/
├── MauiProgram.cs                          DI 登録
├── MainPage.xaml / .xaml.cs                UI（Start / Stop）
├── Services/
│   ├── Audio/
│   │   ├── IAudioStreamSource.cs           共通インターフェース + AudioChunkEventArgs
│   │   ├── AudioStreamSource.ios.cs        iOS 実装（AVAudioEngine + AVAudioConverter）
│   │   └── AudioStreamSource.cs            非 iOS スタブ（PlatformNotSupportedException）
│   ├── RealtimeTranscriber.cs              バッファリング + 定期推論ループ
│   └── Whisper/
│       ├── WhisperModelService.cs          モデルファイル管理（AppPackage → Cache コピー）
│       └── WhisperTranscriptionService.cs  Whisper.net ラッパー
├── Models/
│   └── ggml-small.bin                      ※ 要手動配置（Git 管理外）
└── Resources/Raw/Models/                   MauiAsset としてパッケージに含まれる
```

### 主要クラスの責務

| クラス | 責務 |
|---|---|
| `AudioStreamSource` | マイク取得 → 16 kHz mono Float32 変換 → イベント発火 |
| `RealtimeTranscriber` | 音声バッファ蓄積 → 一定間隔で推論 → `IAsyncEnumerable<string>` で結果を yield |
| `WhisperTranscriptionService` | モデルロード → `ProcessAsync(float[])` 呼び出し → テキスト結合 |
| `WhisperModelService` | AppPackage 内モデルを CacheDirectory にコピー＋パス管理 |
| `MainPage` | Start/Stop ボタン → `RealtimeTranscriber.RunAsync()` の開始・キャンセル |

---

## 前提条件

- **.NET 10 Preview** 以降（`net10.0-ios` ターゲット）
- **Xcode** + iOS 15.0 以上の実機 or シミュレータ
- NuGet パッケージ:
  - `Whisper.net` 1.9.0
  - `Whisper.net.Runtime` 1.9.0

### モデルファイルの配置

`Models/ggml-small.bin` をプロジェクトルートに配置してください。

```
WhisperSample/
  Models/
    ggml-small.bin    ← ここに配置
```

> リポジトリではモデルを Git 管理していないため、未配置でもビルドは通ります。  
> 実行時に `FileNotFoundException` が出る場合はモデルが未配置です。

---

## ビルド・実行

```bash
# iOS 向けビルド
dotnet build -f net10.0-ios -c Debug

# 実機デプロイ（Visual Studio for Mac / VS Code + .NET MAUI 拡張）
# → iOS デバイスを接続して実行
```

---

## 使い方

1. アプリを起動
2. **Start** をタップ → モデルロード → マイク開始 → 推論ループ開始
3. 話す（**5 秒間隔** で推論し、結果を画面に追記）
4. **Stop** で停止

---

## 注意事項

- **マイク権限**: 初回起動時に許可ダイアログが表示されます
  - `Info.plist` に `NSMicrophoneUsageDescription` が必要
  - 設定アプリでマイクが拒否になっていないか確認
- **バッファ戦略**: 推論成功時にバッファをクリアします。連続文を継ぎ足したい場合は `RealtimeTranscriber` のクリアロジックを変更してください
- **iOS 17 deprecation 警告**: `AVAudioSession.RecordPermission` / `RequestRecordPermission` は iOS 17 で非推奨（`AVAudioApplication` への移行推奨）。動作に影響なし

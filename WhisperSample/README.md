# WhisperSample（最小リアルタイム文字起こし）

このプロジェクトは、マイク入力を一定間隔で Whisper に渡して、結果テキストを画面に追記表示する **最小実装** です。

- MVVMなし（コードビハインド直結）
- 未確定/確定の判定なし
- 余計な処理（VAD、差分マージ等）なし

## 前提

- `Models/ggml-small.bin` が **プロジェクト直下** に存在すること
  - 例：`WhisperSample/Models/ggml-small.bin`
  - モデルは「ダウンロード済」とのことなので、この場所に配置してください

> 注: リポジトリによってはモデルをGit管理しないため、未配置でもビルドが通るようにしてあります。

## 使い方

1. アプリを起動

推奨の切り分け手順（どこで落ちているか分かるようにボタンを分けています）:

2. `Load Model` を押す（モデルロード＆Whisper初期化）
3. `Start Mic` を押す（マイク開始）
4. `Start` を押す（推論ループ開始）
5. 話す（1.5秒ごとに推論し、結果を下に追記します）
6. `Stop` で停止

## 実装の要点

- 音声入力: `Services/Audio/AudioStreamSource.ios.cs`
- 推論: `Services/Whisper/WhisperTranscriptionService.cs`
- ストリーミング制御: `Services/RealtimeTranscriber.cs`
- UI: `MainPage.xaml` / `MainPage.xaml.cs`

## 注意

- iOS のマイク許可が必要です（初回起動時に許可ダイアログが出ます）。
- `Start Mic` で落ちる場合は、権限（TCC）まわりの可能性が高いです。
  - `Info.plist` に `NSMicrophoneUsageDescription` があること
  - 設定アプリで WhisperSample のマイクが拒否になっていないこと
- この最小実装は「推論したらバッファクリア」します。連続文を継ぎ足したい場合は `RealtimeTranscriber` のクリア戦略を変えてください。

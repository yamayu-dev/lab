import Foundation
import Speech
@preconcurrency import AVFoundation

/// MAUI/C#から呼び出しやすいラッパークラス
@available(iOS 26.0, *)
@MainActor
@objc(SpeechAnalyzerWrapper)
public final class SpeechAnalyzerWrapper: NSObject {
    private let engine: SpeechAnalyzerEngine
    
    // コールバック用のブロックタイプ
    public typealias ResultCallback = @Sendable (TranscriptSnapshot) -> Void
    public typealias ErrorCallback = @Sendable (String) -> Void
    public typealias StringCallback = @Sendable (String) -> Void
    
    @objc public override init() {
        self.engine = SpeechAnalyzerEngine()
        super.init()
    }
    
    /// リアルタイム音声認識を開始（シンプル版）
    @objc public func startRealtimeWithLocaleIdentifier(
        _ localeIdentifier: String,
        prefix: String,
        onResult: @escaping ResultCallback,
        onError: @escaping ErrorCallback
    ) {
        let locale = Locale(identifier: localeIdentifier)
        engine.startRealtime(
            locale: locale,
            prefix: prefix,
            onResult: onResult,
            onError: onError
        )
    }
    
    /// 音声認識を停止
    @objc public func stop() {
        engine.stop()
    }
    
    /// 音声ファイルから文字起こし（シンプル版）
    @objc public func transcribeFileAtPath(
        _ path: String,
        localeIdentifier: String,
        prefix: String,
        onResult: @escaping ResultCallback,
        onFormatInfo: @escaping StringCallback,
        onComplete: @escaping StringCallback,
        onError: @escaping ErrorCallback
    ) {
        let url = URL(fileURLWithPath: path)
        let locale = Locale(identifier: localeIdentifier)
        
        Task {
            await engine.transcribeFile(
                url: url,
                locale: locale,
                prefix: prefix,
                onResult: onResult,
                onFormatInfo: onFormatInfo,
                onComplete: onComplete,
                onError: onError
            )
        }
    }
    
    /// フォーマット情報を取得
    @objc public var formatInfo: String {
        engine.formatInfo
    }
    
    // MARK: - Static Methods
    
    /// アセットのステータスを確認（日本語用）
    /// 返り値: 1=installed, 2=supported, 3=downloading, 0=other
    @objc public static func checkAssetStatusForJapanese(preset: Int, completion: @escaping (Int) -> Void) {
        let locale = Locale(identifier: "ja-JP")
        let presetValue: SpeechTranscriber.Preset = preset == 0 ? .transcription : .progressiveTranscription
        
        Task {
            let status = await SpeechAnalyzerEngine.assetStatus(locale: locale, preset: presetValue)
            await MainActor.run {
                let statusCode: Int
                switch status {
                case .installed: statusCode = 1
                case .supported: statusCode = 2
                case .downloading: statusCode = 3
                @unknown default: statusCode = 0
                }
                completion(statusCode)
            }
        }
    }
    
    /// アセットをダウンロード（日本語用）
    @objc public static func downloadAssetsForJapanese(
        preset: Int,
        onProgress: @escaping @Sendable (Double) -> Void,
        onError: @escaping @Sendable (String?) -> Void
    ) {
        let locale = Locale(identifier: "ja-JP")
        let presetValue: SpeechTranscriber.Preset = preset == 0 ? .transcription : .progressiveTranscription
        
        Task {
            do {
                try await SpeechAnalyzerEngine.downloadAssets(
                    locale: locale,
                    preset: presetValue,
                    onProgress: onProgress
                )
                await MainActor.run {
                    onError(nil)
                }
            } catch {
                await MainActor.run {
                    onError(error.localizedDescription)
                }
            }
        }
    }
}

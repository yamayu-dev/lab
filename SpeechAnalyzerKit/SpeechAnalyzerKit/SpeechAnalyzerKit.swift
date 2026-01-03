//
//  SpeechAnalyzerKit.swift
//  SpeechAnalyzerKit
//
//  Created by 山本悠貴 on 2026/01/03.
//

import Foundation
import Speech

/// SpeechAnalyzerKitのメインインターフェース
@objc(SpeechAnalyzerKitHelper)
public final class SpeechAnalyzerKitHelper: NSObject {
    
    /// 音声認識の許可状態を確認
    @objc public static var authorizationStatus: SFSpeechRecognizerAuthorizationStatus {
        SFSpeechRecognizer.authorizationStatus()
    }
    
    /// 音声認識の許可をリクエスト
    @objc public static func requestAuthorization(completion: @escaping (SFSpeechRecognizerAuthorizationStatus) -> Void) {
        SFSpeechRecognizer.requestAuthorization(completion)
    }
    
    /// 録音の許可をリクエスト（iOS 17.0以降）
    @available(iOS 17.0, *)
    @objc public static func requestRecordPermission(completion: @escaping (Bool) -> Void) {
        AVAudioApplication.requestRecordPermission(completionHandler: completion)
    }
    
    /// 録音の許可をリクエスト（iOS 17.0未満）
    @objc public static func requestRecordPermissionLegacy(completion: @escaping (Bool) -> Void) {
        AVAudioSession.sharedInstance().requestRecordPermission(completion)
    }
    
    /// SpeechAnalyzerEngineを作成（iOS 26.0以降）
    @available(iOS 26.0, *)
    @MainActor
    public static func createEngine() -> SpeechAnalyzerEngine {
        return SpeechAnalyzerEngine()
    }
    
    /// AudioRecorderを作成
    @MainActor
    @objc public static func createAudioRecorder() -> AudioRecorder {
        return AudioRecorder()
    }
    
    /// 利用可能なロケールを取得
    @objc public static func availableLocales() -> [Locale] {
        return SFSpeechRecognizer.supportedLocales().sorted { locale1, locale2 in
            let name1 = locale1.localizedString(forIdentifier: locale1.identifier) ?? locale1.identifier
            let name2 = locale2.localizedString(forIdentifier: locale2.identifier) ?? locale2.identifier
            return name1 < name2
        }
    }
    
    /// 特定のロケールが利用可能かチェック
    @objc public static func isLocaleSupported(_ locale: Locale) -> Bool {
        return SFSpeechRecognizer(locale: locale) != nil
    }
}

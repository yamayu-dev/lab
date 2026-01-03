import Foundation
@preconcurrency import AVFoundation

/// 音声録音機能を提供するクラス
@MainActor
@objc(AudioRecorder)
public final class AudioRecorder: NSObject {
    private var audioRecorder: AVAudioRecorder?
    
    @objc public var audioFileURL: URL?
    @objc public var audioDisplayName: String = ""
    
    @objc public var isRecording: Bool {
        audioRecorder?.isRecording ?? false
    }
    
    @objc public override init() {
        super.init()
    }
    
    /// 録音を開始
    @objc public func startRecording() throws {
        let session = AVAudioSession.sharedInstance()
        try session.setCategory(.playAndRecord, mode: .spokenAudio, options: [.defaultToSpeaker])
        try session.setActive(true)
        
        let fileURL = FileManager.default.temporaryDirectory
            .appendingPathComponent("recording")
            .appendingPathExtension("m4a")
        
        let settings: [String: Any] = [
            AVFormatIDKey: Int(kAudioFormatMPEG4AAC),
            AVSampleRateKey: 44100,
            AVNumberOfChannelsKey: 1,
            AVEncoderAudioQualityKey: AVAudioQuality.high.rawValue,
        ]
        
        audioRecorder = try AVAudioRecorder(url: fileURL, settings: settings)
        audioRecorder?.prepareToRecord()
        audioRecorder?.record()
        audioFileURL = fileURL
        audioDisplayName = "recording.m4a"
    }
    
    /// 録音を停止
    @objc public func stopRecording() {
        audioRecorder?.stop()
        audioRecorder = nil
    }
    
    /// 録音データを削除
    @objc public func deleteAudio() {
        if let url = audioFileURL {
            try? FileManager.default.removeItem(at: url)
        }
        audioFileURL = nil
        audioDisplayName = ""
    }
    
    /// 外部の音声ファイルを設定
    @objc public func setAudio(from url: URL) throws {
        let fileExtension = url.pathExtension.isEmpty ? "m4a" : url.pathExtension
        let tempURL = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
            .appendingPathExtension(fileExtension)
        
        let didStartAccess = url.startAccessingSecurityScopedResource()
        defer {
            if didStartAccess {
                url.stopAccessingSecurityScopedResource()
            }
        }
        
        if FileManager.default.fileExists(atPath: tempURL.path) {
            try FileManager.default.removeItem(at: tempURL)
        }
        try FileManager.default.copyItem(at: url, to: tempURL)
        deleteAudio()
        audioFileURL = tempURL
        audioDisplayName = url.lastPathComponent
    }
    
    /// エクスポート用のファイル名を取得
    @objc public var suggestedExportFileName: String {
        let baseName: String
        if !audioDisplayName.isEmpty {
            baseName = (audioDisplayName as NSString).deletingPathExtension
        } else {
            baseName = "recording"
        }
        return "\(baseName).m4a"
    }
}

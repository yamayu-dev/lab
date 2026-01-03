import Foundation
import Speech
@preconcurrency import AVFoundation
import CoreMedia

/// SpeechAnalyzer（iOS 26.0以降）を使った音声認識エンジン
@available(iOS 26.0, *)
@MainActor
@objc(SpeechAnalyzerEngine)
public final class SpeechAnalyzerEngine: NSObject {
    private let audioEngine = AVAudioEngine()
    private var analyzeTask: Task<Void, Never>?
    private var resultsTask: Task<Void, Never>?
    private var finishInput: (() -> Void)?
    
    private var finalizedLines: [String] = []
    private var volatileText: String = ""
    private var lastFormatInfo: String = ""
    
    nonisolated private final class AnalyzerInputContinuationBox: @unchecked Sendable {
        var continuation: AsyncThrowingStream<AnalyzerInput, Error>.Continuation?
    }
    
    private struct UncheckedSendable<T>: @unchecked Sendable {
        let value: T
    }
    
    @objc public var formatInfo: String {
        lastFormatInfo
    }
    
    @objc public override init() {
        super.init()
    }
    
    // MARK: - Realtime
    
    /// リアルタイム音声認識を開始
    public func startRealtime(
        locale: Locale,
        prefix: String = "",
        onResult: @escaping (TranscriptSnapshot) -> Void,
        onError: @escaping (String) -> Void
    ) {
        stop()
        
        finalizedLines = []
        volatileText = ""
        
        guard SFSpeechRecognizer.authorizationStatus() == .authorized else {
            onError("音声認識の許可が必要です。")
            return
        }
        
        let session = AVAudioSession.sharedInstance()
        do {
            try session.setCategory(.record, mode: .measurement, options: [.duckOthers])
            try session.setActive(true, options: .notifyOthersOnDeactivation)
        } catch {
            onError("オーディオ設定に失敗しました。")
            return
        }
        
        analyzeTask = Task { [weak self] in
            guard let self else { return }
            await self.runRealtimeAnalysis(locale: locale, prefix: prefix, onResult: onResult, onError: onError)
        }
    }
    
    private func runRealtimeAnalysis(
        locale: Locale,
        prefix: String,
        onResult: @escaping (TranscriptSnapshot) -> Void,
        onError: @escaping (String) -> Void
    ) async {
        do {
            // プログレッシブ（低レイテンシ）プリセットを使用
            let transcriber = SpeechTranscriber(
                locale: locale,
                preset: .progressiveTranscription
            )
            
            let status = await AssetInventory.status(forModules: [transcriber])
            guard status == .installed else {
                throw NSError(
                    domain: "SpeechAnalyzerKit",
                    code: 10,
                    userInfo: [NSLocalizedDescriptionKey: "SpeechAnalyzerアセットが未インストールです。設定からダウンロードしてください。"]
                )
            }
            
            let analyzer = SpeechAnalyzer(modules: [transcriber])
            
            let compatibleFormats = await transcriber.availableCompatibleAudioFormats
            var preferredTapFormat: AVAudioFormat?
            if let int16Mono = compatibleFormats.first(where: { $0.commonFormat == .pcmFormatInt16 && $0.channelCount == 1 }) {
                preferredTapFormat = int16Mono
            } else if let int16Any = compatibleFormats.first(where: { $0.commonFormat == .pcmFormatInt16 }) {
                preferredTapFormat = int16Any
            } else {
                preferredTapFormat = compatibleFormats.sorted(by: { $0.sampleRate < $1.sampleRate }).first
            }
            
            let box = AnalyzerInputContinuationBox()
            let inputStream = AsyncThrowingStream<AnalyzerInput, Error> { cont in
                box.continuation = cont
            }
            self.finishInput = {
                box.continuation?.finish()
            }
            
            let yieldInput: (AVAudioPCMBuffer) -> Void = { buffer in
                box.continuation?.yield(AnalyzerInput(buffer: buffer))
            }
            
            self.resultsTask = Task { @MainActor in
                do {
                    for try await result in transcriber.results {
                        let snapshot = self.processResult(result, prefix: prefix)
                        onResult(snapshot)
                    }
                } catch {
                    // 停止扱い
                }
            }
            
            let inputNode = audioEngine.inputNode
            let recordingFormat = inputNode.outputFormat(forBus: 0)
            
            let tapFormat: AVAudioFormat = {
                if let preferredTapFormat {
                    if preferredTapFormat.commonFormat == .pcmFormatInt16 {
                        return preferredTapFormat
                    }
                    if let converted = AVAudioFormat(
                        commonFormat: .pcmFormatInt16,
                        sampleRate: preferredTapFormat.sampleRate,
                        channels: preferredTapFormat.channelCount,
                        interleaved: false
                    ) {
                        return converted
                    }
                    return preferredTapFormat
                }
                
                return AVAudioFormat(
                    commonFormat: .pcmFormatInt16,
                    sampleRate: recordingFormat.sampleRate,
                    channels: recordingFormat.channelCount,
                    interleaved: false
                ) ?? recordingFormat
            }()
            
            guard let converter = AVAudioConverter(from: recordingFormat, to: tapFormat) else {
                throw NSError(domain: "SpeechAnalyzerKit", code: 2, userInfo: [NSLocalizedDescriptionKey: "音声フォーマット変換器を作成できません。"])
            }
            
            inputNode.removeTap(onBus: 0)
            inputNode.installTap(onBus: 0, bufferSize: 1024, format: recordingFormat) { buffer, _ in
                let ratio = tapFormat.sampleRate / recordingFormat.sampleRate
                let estimatedFrames = AVAudioFrameCount(Double(buffer.frameLength) * ratio) + 1
                guard let outputBuffer = AVAudioPCMBuffer(pcmFormat: tapFormat, frameCapacity: estimatedFrames) else {
                    return
                }
                
                var error: NSError?
                var didProvideInput = false
                let input = UncheckedSendable(value: buffer)
                let status = converter.convert(to: outputBuffer, error: &error) { _, outStatus in
                    if didProvideInput {
                        outStatus.pointee = .noDataNow
                        return nil
                    }
                    didProvideInput = true
                    outStatus.pointee = .haveData
                    return input.value
                }
                
                if error == nil, status != .error, outputBuffer.frameLength > 0 {
                    yieldInput(outputBuffer)
                }
            }
            
            audioEngine.prepare()
            try audioEngine.start()
            
            let lastSampleTime = try await analyzer.analyzeSequence(inputStream)
            if let lastSampleTime {
                try? await analyzer.finalizeAndFinish(through: lastSampleTime)
            } else {
                await analyzer.cancelAndFinishNow()
            }
            
            await MainActor.run {
                onError("停止")
            }
        } catch {
            await MainActor.run {
                if Task.isCancelled || self.isCancellation(error) {
                    onError("停止")
                } else {
                    onError("SpeechAnalyzerでの認識に失敗しました: \(error.localizedDescription)")
                }
            }
        }
    }
    
    /// 音声認識を停止
    @objc public func stop() {
        if audioEngine.isRunning {
            audioEngine.stop()
            audioEngine.inputNode.removeTap(onBus: 0)
        }
        finishInput?()
        finishInput = nil
        resultsTask = nil
        analyzeTask = nil
    }
    
    // MARK: - File
    
    /// 音声ファイルから文字起こし
    nonisolated public func transcribeFile(
        url: URL,
        locale: Locale,
        prefix: String = "",
        onResult: @MainActor @escaping (TranscriptSnapshot) -> Void,
        onFormatInfo: @MainActor @escaping (String) -> Void,
        onComplete: @MainActor @escaping (String) -> Void,
        onError: @MainActor @escaping (String) -> Void
    ) async {
        do {
            let session = AVAudioSession.sharedInstance()
            do {
                try session.setCategory(.playback, mode: .spokenAudio, options: [])
                try session.setActive(true)
            } catch {
                // 続行
            }
            
            let transcriber = SpeechTranscriber(locale: locale, preset: .transcription)
            let status = await AssetInventory.status(forModules: [transcriber])
            guard status == .installed else {
                throw NSError(
                    domain: "SpeechAnalyzerKit",
                    code: 11,
                    userInfo: [NSLocalizedDescriptionKey: "SpeechAnalyzerアセットが未インストールです。設定からダウンロードしてください。"]
                )
            }
            
            let analyzer = SpeechAnalyzer(modules: [transcriber])
            
            let audioFile = try AVAudioFile(forReading: url)
            let sourceFormat = audioFile.processingFormat
            let info = "src: \(self.audioFormatDescription(sourceFormat))"
            
            await MainActor.run {
                self.lastFormatInfo = info
                onFormatInfo(info)
            }
            
            await MainActor.run {
                self.finalizedLines = []
                self.volatileText = ""
                self.resultsTask = Task { @MainActor in
                    do {
                        for try await result in transcriber.results {
                            let snapshot = self.processResult(result, prefix: prefix)
                            onResult(snapshot)
                        }
                    } catch {
                        // 後段で扱う
                    }
                }
            }
            
            let lastSampleTime = try await analyzer.analyzeSequence(from: audioFile)
            if let lastSampleTime {
                try? await analyzer.finalizeAndFinish(through: lastSampleTime)
            } else {
                await analyzer.cancelAndFinishNow()
            }
            
            await MainActor.run {
                let suffix = self.lastFormatInfo.isEmpty ? "" : " (\(self.lastFormatInfo))"
                onComplete("変換完了 (SpeechAnalyzer)" + suffix)
            }
        } catch {
            await MainActor.run {
                if Task.isCancelled || self.isCancellation(error) {
                    onError("停止")
                } else {
                    let suffix = self.lastFormatInfo.isEmpty ? "" : " (\(self.lastFormatInfo))"
                    let ns = error as NSError
                    let detail = "\(ns.domain)(\(ns.code))"
                    onError("SpeechAnalyzerでの変換に失敗しました: \(error.localizedDescription) [\(detail)]" + suffix)
                }
            }
        }
    }
    
    // MARK: - Assets
    
    /// アセットのステータスを確認
    public static func assetStatus(locale: Locale, preset: SpeechTranscriber.Preset) async -> AssetInventory.Status {
        let transcriber = SpeechTranscriber(locale: locale, preset: preset)
        return await AssetInventory.status(forModules: [transcriber])
    }
    
    /// アセットをダウンロード
    public static func downloadAssets(
        locale: Locale,
        preset: SpeechTranscriber.Preset,
        onProgress: @MainActor @escaping (Double) -> Void
    ) async throws {
        let transcriber = SpeechTranscriber(locale: locale, preset: preset)
        let status = await AssetInventory.status(forModules: [transcriber])
        
        if status == .installed {
            await MainActor.run { onProgress(1.0) }
            return
        }
        
        guard status == .supported || status == .downloading else {
            throw NSError(domain: "SpeechAnalyzerKit", code: 12, userInfo: [NSLocalizedDescriptionKey: "この言語のSpeechAnalyzerアセットは利用できません。"])
        }
        
        guard let request = try await AssetInventory.assetInstallationRequest(supporting: [transcriber]) else {
            await MainActor.run { onProgress(1.0) }
            return
        }
        
        let progress = request.progress
        let pollTask = Task { @MainActor in
            while !Task.isCancelled {
                onProgress(progress.fractionCompleted)
                if progress.isFinished {
                    break
                }
                try? await Task.sleep(nanoseconds: 250_000_000)
            }
        }
        defer { pollTask.cancel() }
        
        try await request.downloadAndInstall()
        await MainActor.run { onProgress(1.0) }
    }
    
    // MARK: - Private
    
    // isFinalフラグで判定
    private func processResult(_ result: SpeechTranscriber.Result, prefix: String) -> TranscriptSnapshot {
        let recognized = String(result.text.characters)
        let trimmed = recognized.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else {
            return buildTranscript(prefix: prefix)
        }
        
        if result.isFinal {
            // 確定したテキスト → 行として追加
            finalizedLines.append(recognized)
            volatileText = ""
        } else {
            // 揮発性（部分的な）テキスト
            volatileText = recognized
        }
        
        return buildTranscript(prefix: prefix)
    }
    
    private func buildTranscript(prefix: String) -> TranscriptSnapshot {
        let confirmed = finalizedLines.joined(separator: "\n")
        let body: String
        if volatileText.isEmpty {
            body = confirmed
        } else {
            body = confirmed + (confirmed.isEmpty ? "" : "\n") + volatileText
        }
        let combined = prefix.isEmpty ? body : (prefix + body)
        let confirmedWithPrefix = prefix.isEmpty ? confirmed : (prefix + confirmed)
        return TranscriptSnapshot(
            confirmedText: confirmedWithPrefix,
            pendingText: volatileText,
            combinedText: combined
        )
    }
    
    private nonisolated func audioFormatDescription(_ format: AVAudioFormat) -> String {
        let common: String
        switch format.commonFormat {
        case .pcmFormatFloat32: common = "f32"
        case .pcmFormatFloat64: common = "f64"
        case .pcmFormatInt16: common = "i16"
        case .pcmFormatInt32: common = "i32"
        case .otherFormat: common = "other"
        @unknown default: common = "unknown"
        }
        
        return "\(Int(format.sampleRate))Hz \(format.channelCount)ch \(common) \(format.isInterleaved ? "interleaved" : "non-interleaved")"
    }
    
    private func isCancellation(_ error: Error) -> Bool {
        if error is CancellationError {
            return true
        }
        
        let nsError = error as NSError
        if nsError.domain == NSCocoaErrorDomain, nsError.code == NSUserCancelledError {
            return true
        }
        
        if nsError.domain == NSURLErrorDomain, nsError.code == URLError.cancelled.rawValue {
            return true
        }
        
        return false
    }
}

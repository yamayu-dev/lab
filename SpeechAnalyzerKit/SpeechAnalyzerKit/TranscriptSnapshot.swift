import Foundation

/// 音声認識の結果のスナップショット
@objc(TranscriptSnapshot)
public class TranscriptSnapshot: NSObject {
    /// 確定されたテキスト
    @objc public let confirmedText: String
    /// 暫定的なテキスト（揮発性）
    @objc public let pendingText: String
    /// 確定テキストと暫定テキストを結合したもの
    @objc public let combinedText: String
    
    @objc public init(confirmedText: String, pendingText: String, combinedText: String) {
        self.confirmedText = confirmedText
        self.pendingText = pendingText
        self.combinedText = combinedText
    }
}

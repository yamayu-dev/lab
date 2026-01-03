using MauiSpeechAnalyzer.Services;

namespace MauiSpeechAnalyzer
{
    public partial class MainPage : ContentPage
    {
        private readonly ISpeechRecognitionService _speechService;

        public MainPage()
        {
            InitializeComponent();
            _speechService = new SpeechRecognitionService();
        }

        private async void OnStartClicked(object? sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "権限確認中...";

                var hasPermission = await _speechService.CheckPermissionsAsync();
                if (!hasPermission)
                {
                    StatusLabel.Text = "エラー: マイクまたは音声認識の権限が必要です";
                    return;
                }

                StatusLabel.Text = "音声認識を開始しました";
                StartBtn.IsEnabled = false;
                StopBtn.IsEnabled = true;

                _speechService.StartRecognition(
                    "ja-JP",
                    (confirmed, pending, combined) =>
                    {
                        ConfirmedLabel.Text = confirmed;
                        PendingLabel.Text = pending;
                        CombinedLabel.Text = combined;
                    },
                    error =>
                    {
                        StatusLabel.Text = $"エラー: {error}";
                        StartBtn.IsEnabled = true;
                        StopBtn.IsEnabled = false;
                    }
                );
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"エラー: {ex.Message}";
                StartBtn.IsEnabled = true;
                StopBtn.IsEnabled = false;
            }
        }

        private void OnStopClicked(object? sender, EventArgs e)
        {
            try
            {
                _speechService.StopRecognition();
                StatusLabel.Text = "停止しました";
                StartBtn.IsEnabled = true;
                StopBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"エラー: {ex.Message}";
            }
        }

        private async void OnCheckAssetClicked(object? sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "アセット状態を確認中...";
                CheckAssetBtn.IsEnabled = false;

                var status = await _speechService.CheckAssetStatusAsync();
                var statusText = status switch
                {
                    1 => "インストール済み",
                    2 => "サポート済み（ダウンロード必要）",
                    3 => "ダウンロード中",
                    _ => "不明"
                };

                StatusLabel.Text = $"アセット状態: {statusText}";

                if (status == 2)
                {
                    var download = await DisplayAlert("ダウンロード", "アセットをダウンロードしますか？", "はい", "いいえ");
                    if (download)
                    {
                        await DownloadAssetsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"エラー: {ex.Message}";
            }
            finally
            {
                CheckAssetBtn.IsEnabled = true;
            }
        }

        private async Task DownloadAssetsAsync()
        {
            try
            {
                StatusLabel.Text = "ダウンロード中... 0%";

                await _speechService.DownloadAssetsAsync(progress =>
                {
                    StatusLabel.Text = $"ダウンロード中... {progress * 100:F0}%";
                });

                StatusLabel.Text = "ダウンロード完了！";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"ダウンロードエラー: {ex.Message}";
            }
        }
    }
}

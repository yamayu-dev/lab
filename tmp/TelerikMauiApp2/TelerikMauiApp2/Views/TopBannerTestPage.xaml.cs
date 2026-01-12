using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using TelerikMauiApp2.Services;

namespace TelerikMauiApp2.Views
{
    public partial class TopBannerTestPage : ContentPage
    {
        CancellationTokenSource? _topSnackbarCts;
        Func<Task>? _topSnackbarAction;

        private TopBannerWindowOverlayService _windowOverlayBanner;

        public TopBannerTestPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ApplyTopSafeAreaInset();
            
            // WindowOverlay の初期化
            // レイアウト完了後に SafeAreaTop を設定
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                var window = this.Window;
                if (window is null) return;

                _windowOverlayBanner = new TopBannerWindowOverlayService(window);
                _windowOverlayBanner.Add();
            });
        }

        private async Task ShowTopSnackbarAsync(
            string message,
            int durationMs = 3000,
            string? actionText = null,
            Func<Task>? action = null)
        {
            ApplyTopSafeAreaInset();

            _topSnackbarCts?.Cancel();
            _topSnackbarCts = new CancellationTokenSource();
            var token = _topSnackbarCts.Token;

            _topSnackbarAction = action;

            TopSnackbarText.Text = message;
            TopSnackbarActionButton.Text = string.IsNullOrWhiteSpace(actionText) ? "OK" : actionText;
            TopSnackbarActionButton.IsVisible = actionText != null;

            TopSnackbarHost.Opacity = 1;
            TopSnackbarHost.IsVisible = true;

            TopSnackbar.Opacity = 0;
            TopSnackbar.TranslationY = -10;

            await Task.WhenAll(
                TopSnackbar.FadeTo(1, 140, Easing.CubicOut),
                TopSnackbar.TranslateTo(0, 0, 140, Easing.CubicOut));

            try
            {
                await Task.Delay(durationMs, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            await DismissTopSnackbarAsync(token);
        }

        private void ApplyTopSafeAreaInset()
        {
            // NOTE:
            // "セーフエリアの少し下"に出したい場合、上部インセット + 追加マージン(px) を足してスペーサーで下げる。
            // 
            // iOS: 本来は SafeAreaInsets(top) を反映するのがベスト。
            // Android/Windows: まずは少しだけ余白を入れる（必要なら後で取得方法を詰める）。

            // セーフエリア「ぎりぎり」に寄せたいので extra は 0。
            // もし「少し下」にしたい場合は 2〜8 あたりを足すと良い。
            var extra = 0d;
            var insetTop = 0d;

#if IOS
            // iOS セーフエリア:
            // このプロジェクトの MAUI/ターゲットでは Window.SafeAreaInsets が使えないため、
            // SafeArea 対応済みの Page.Padding から上部インセットを取得する。
            // 端末の向き変更時も Padding が更新される想定なので、このメソッドを再実行すれば追従する。
            insetTop = this.Padding.Top;
#endif

            // 他プラットフォームは、まず固定の少しだけ下げる。
            // 必要になったら platform handler で Insets を取得するように拡張できる。
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                insetTop = 0;
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                insetTop = 0;
            }

            TopSnackbarSafeAreaSpacer.HeightRequest = insetTop + extra;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // 端末の回転などでセーフエリアが変わるので都度反映
            ApplyTopSafeAreaInset();

            // WindowOverlay 側はサービス内部で SafeArea を再計算するため、ここでの注入は不要。
        }

        private async Task DismissTopSnackbarAsync(CancellationToken token)
        {
            await Task.WhenAll(
                TopSnackbar.FadeTo(0, 120, Easing.CubicIn),
                TopSnackbar.TranslateTo(0, -10, 120, Easing.CubicIn));

            if (!token.IsCancellationRequested)
            {
                TopSnackbarHost.IsVisible = false;
                _topSnackbarAction = null;
            }
        }

        private async void OnTopSnackbarActionClicked(object sender, EventArgs e)
        {
            // アクション実行 -> 即閉じ
            var action = _topSnackbarAction;
            _topSnackbarAction = null;

            if (action != null)
            {
                await action();
            }

            _topSnackbarCts?.Cancel();
            _topSnackbarCts = new CancellationTokenSource();
            await DismissTopSnackbarAsync(_topSnackbarCts.Token);
        }

        private async void OnShowToolkitTopToastClicked(object sender, EventArgs e)
        {
            // NOTE: CommunityToolkitのToastは「画面のどこに出すか」を指定できません。
            // 上部に見せたい場合は「自前オーバーレイ」か、下の Snackbar のカスタム表示を使うのが現実的です。
            var toast = Toast.Make(
                "Toolkit Toast（位置指定不可なので通常位置で表示）",
                ToastDuration.Short);

            await toast.Show();
        }

        private async void OnShowToolkitTopSnackbarClicked(object sender, EventArgs e)
        {
            // CommunityToolkit Snackbar
            // NOTE:
            // CommunityToolkitのSnackbarは基本的に「画面下部」に表示され、位置指定APIはありません。
            // 「上部固定」をやりたい場合は、次のステップでページの最上位にオーバーレイ表示(自前)を実装するのが確実です。

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Colors.Black,
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.White,
                CornerRadius = new CornerRadius(12),
                Font = Microsoft.Maui.Font.SystemFontOfSize(14),
                ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(
                "Toolkit Snackbar（位置指定不可なので通常位置で表示）",
                action: null,
                actionButtonText: "OK",
                duration: TimeSpan.FromSeconds(3),
                visualOptions: snackbarOptions);

            await snackbar.Show();
        }

        private async void OnShowTopBannerClicked(object sender, EventArgs e)
        {
            await ShowTopSnackbarAsync("上部固定 Snackbar（自前オーバーレイ / 3秒）", 3000);
        }

        private void OnShowTopBannerViaWindowOverlayClicked(object sender, EventArgs e)
        {
            _windowOverlayBanner?.Show("上部固定バナー通知（Window Overlay / 2.5秒）");
        }
    }
}

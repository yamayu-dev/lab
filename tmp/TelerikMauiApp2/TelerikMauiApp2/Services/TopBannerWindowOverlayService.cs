using Microsoft.Maui;
using Microsoft.Maui.Controls;
using TelerikMauiApp2.Controls;

namespace TelerikMauiApp2.Services;

public class TopBannerWindowOverlayService
{
    private readonly IWindow _window;
    private readonly WindowOverlay _overlay;
    private readonly TopBannerOverlayElement _element;

    /// <summary>
    /// 追加マージン（px）。セーフエリアぴったりではなく少し下に出したい場合に使う。
    /// </summary>
    public double ExtraTopMargin { get; set; } = 0;

    public TopBannerWindowOverlayService(IWindow window)
    {
        _window = window;

        // オーバーレイ生成
        _overlay = new WindowOverlay(window)
        {
            // 画面全体をブロックしないようにパススルーを許可
            DisableUITouchEventPassthrough = false,
            IsVisible = false
        };

        // バナー要素
        _element = new TopBannerOverlayElement();
        try
        {
            _overlay.AddWindowElement(_element);
        }
        catch
        {
            // Overlay 要素の追加に失敗してもクラッシュさせない
        }

        // オーバーレイ Tap イベント（要素外クリックも拾う）
        _overlay.Tapped += (s, e) =>
        {
            // 要素をタップしたら閉じるなど
            Hide();
        };
    }

    public void Show(string message, double durationSeconds = 2.5)
    {
        try
        {
            _element.SetSafeAreaTop(GetSafeAreaTop() + ExtraTopMargin);
            _element.SetMessage(message);
            _overlay.IsVisible = true;
            _overlay.Invalidate();  // 再描画
            AnimateHide(durationSeconds);
        }
        catch
        {
            // オーバーレイ未初期化やプラットフォーム未対応時に落ちないよう保護
        }
    }

    private async void AnimateHide(double durationSeconds)
    {
        // 一定時間後に非表示
        await Task.Delay(TimeSpan.FromSeconds(durationSeconds));
        Hide();
    }

    public void Hide()
    {
        try
        {
            _overlay.IsVisible = false;
        }
        catch
        {
            // ignore
        }
    }

    public void Add()
    {
        try
        {
            _window.AddOverlay(_overlay);       // Window に追加
        }
        catch
        {
            // ignore
        }
    }

    public void Remove()
    {
        try
        {
            _window.RemoveOverlay(_overlay); // Window から除去
        }
        catch
        {
            // ignore
        }
    }

    private double GetSafeAreaTop()
    {
        // どの画面から呼んでも動くように、サービス内部でプラットフォームごとに取得する。
        // iOS は StatusBar/Notch の変化があるので SafeAreaInsets.Top を使う。
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
#if IOS
            try
            {
                // iOS 13+ はマルチシーンのため KeyWindow は不正確になりがち。
                // Foreground の UIWindowScene から key window を探して SafeAreaInsets を取る。
                var keyWindow = UIKit.UIApplication.SharedApplication
                    .ConnectedScenes
                    .OfType<UIKit.UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);

                return keyWindow?.SafeAreaInsets.Top ?? 0;
            }
            catch
            {
                return 0;
            }
#else
            return 0;
#endif
        }

        // Android/Windows/MacCatalyst: ひとまず 0（必要なら将来的に platform handler で拡張）
        return 0;
    }
}


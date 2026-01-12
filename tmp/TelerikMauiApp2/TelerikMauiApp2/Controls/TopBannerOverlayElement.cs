using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace TelerikMauiApp2.Controls;

public class TopBannerOverlayElement : IWindowOverlayElement
{
    private RectF bannerRect;
    private string _text = "バナー通知";

    public float SafeAreaTop { get; set; } = 0;
    public float BannerHeight { get; set; } = 68;
    public float HorizontalPadding { get; set; } = 16;
    public float VerticalPadding { get; set; } = 8;
    public float CornerRadius { get; set; } = 12;
    public float FontSize { get; set; } = 16; // アプリ標準に近いサイズ

    public Action? TappedAction { get; set; }

    public bool Contains(Point point)
    {
        // バナー領域のみタッチを拾う
        return bannerRect.Contains(point);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float width = dirtyRect.Width;

        // SafeAreaTopの下にVerticalPaddingを追加した位置からバナーを表示
        var top = SafeAreaTop + VerticalPadding;
        bannerRect = new RectF(HorizontalPadding, top, width - (HorizontalPadding * 2), BannerHeight);

        // 背景
        canvas.FillColor = Colors.Black;
        canvas.FillRoundedRectangle(bannerRect, CornerRadius);

        // テキスト
        canvas.FontColor = Colors.White;
        canvas.FontSize = FontSize;
        canvas.DrawString(_text, bannerRect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    public void SetMessage(string text)
    {
        _text = text;
    }

    public void ApplySafeAreaInsets(Thickness safeAreaInsets)
    {
        SafeAreaTop = (float)safeAreaInsets.Top;
    }

    public void SetSafeAreaTop(double safeAreaTop)
    {
        SafeAreaTop = (float)safeAreaTop;
    }
}


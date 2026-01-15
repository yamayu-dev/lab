using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Extensions;

internal static class EmojiFontImageSourceExtensions
{
    // Windows/Android/iOS で「絵文字1文字」を ImageSource 化するための簡易ヘルパ。
    // フォントは OS のデフォルトに任せる（絵文字フォントが無い環境では表示されない可能性あり）。
    public static ImageSource ToFontImageSource(this string glyph, double size = 18)
    {
        return new FontImageSource
        {
            Glyph = glyph,
            Size = size,
        };
    }
}

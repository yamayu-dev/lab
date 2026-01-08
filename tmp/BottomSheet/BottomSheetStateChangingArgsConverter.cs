using System.Globalization;
using Telerik.Maui.Controls.BottomSheet;

namespace TelerikMauiApp2.Converters;

/// <summary>
/// RadBottomSheet の StateChanging イベント引数を ViewModel 側で扱いやすい DTO に変換します。
/// </summary>
public sealed class BottomSheetStateChangingArgsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BottomSheetStateChangingEventArgs e)
        {
            return null;
        }

        return new BottomSheetStateChangeInfo
        {
            Position = e.Position,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class BottomSheetStateChangeInfo
{
    public double Position { get; init; }
}

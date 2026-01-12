using System.Globalization;

namespace TelerikMauiApp2.Converters;

public sealed class IndexEqualsSelectedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is int index)
        {
            return selectedIndex == index;
        }

        if (value is int si && parameter is string s && int.TryParse(s, out var i))
        {
            return si == i;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

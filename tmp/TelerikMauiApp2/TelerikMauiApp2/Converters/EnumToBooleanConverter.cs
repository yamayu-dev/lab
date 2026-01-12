using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Converters
{
    /// <summary>
    /// Enum値とパラメータを比較してboolを返すコンバーター。
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            return value.Equals(parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return parameter;
            }

            return Binding.DoNothing;
        }
    }
}

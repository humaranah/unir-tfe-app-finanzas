using System.Globalization;

namespace HA.TFG.AppFinanzas.App.Converters;

public class FilaParColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isEven = value is bool b && b;
        if (!isEven)
            return Colors.Transparent;

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return isDark ? Color.FromArgb("#252525") : Color.FromArgb("#F0F0F0");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

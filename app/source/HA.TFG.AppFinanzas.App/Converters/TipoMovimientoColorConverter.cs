using HA.TFG.AppFinanzas.Core.Models.Enums;
using System.Globalization;

namespace HA.TFG.AppFinanzas.App.Converters;

public class TipoMovimientoColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TipoMovimiento tipo)
            return Colors.Transparent;

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        return tipo switch
        {
            TipoMovimiento.Ingreso => isDark ? Color.FromArgb("#66BB6A") : Color.FromArgb("#2E7D32"),
            TipoMovimiento.Gasto => isDark ? Color.FromArgb("#FF6B6B") : Color.FromArgb("#D32F2F"),
            TipoMovimiento.Transferencia => isDark ? Color.FromArgb("#CE93D8") : Color.FromArgb("#6A1B9A"),
            _ => Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

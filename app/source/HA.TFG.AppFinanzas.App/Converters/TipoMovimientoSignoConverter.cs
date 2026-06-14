using HA.TFG.AppFinanzas.Core.Models.Enums;
using System.Globalization;

namespace HA.TFG.AppFinanzas.App.Converters;

public class TipoMovimientoSignoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TipoMovimiento.Ingreso ? string.Empty : "-";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

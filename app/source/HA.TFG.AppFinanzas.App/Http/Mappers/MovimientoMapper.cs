using HA.TFG.AppFinanzas.App.Http;
using HA.TFG.AppFinanzas.Core.Movimientos;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.App.Http.Mappers;

[Mapper]
internal static partial class MovimientoMapper
{
    internal static partial MovimientoItem ToMovimientoItem(MovimientoResponse response);
}

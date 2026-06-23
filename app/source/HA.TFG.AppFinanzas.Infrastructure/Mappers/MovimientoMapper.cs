using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Infrastructure.Clients;
using Riok.Mapperly.Abstractions;

namespace HA.TFG.AppFinanzas.Infrastructure.Mappers;

[Mapper]
public static partial class MovimientoMapper
{
    public static partial MovimientoItem ToMovimientoItem(MovimientoResponse response);
}

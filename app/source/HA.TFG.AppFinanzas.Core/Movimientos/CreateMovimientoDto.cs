using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed record CreateMovimientoDto(
    Guid IdCuenta,
    string Concepto,
    decimal Importe,
    string Moneda,
    TipoMovimiento Tipo,
    DateTime FechaHora,
    Guid IdCuentaCategoria,
    string? Establecimiento = null,
    string? Notas = null);

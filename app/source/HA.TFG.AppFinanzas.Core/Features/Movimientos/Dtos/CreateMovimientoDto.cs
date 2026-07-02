using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Features.Movimientos.Dtos;

public sealed record CreateMovimientoDto(
    Guid IdCuenta,
    string Concepto,
    decimal Importe,
    string Moneda,
    TipoMovimiento Tipo,
    DateTime FechaHora,
    Guid IdCuentaCategoria,
    string? Establecimiento = null,
    string? Nota = null,
    byte[]? ComprobanteBytes = null,
    string? ComprobanteNombre = null,
    string? ComprobanteContentType = null);

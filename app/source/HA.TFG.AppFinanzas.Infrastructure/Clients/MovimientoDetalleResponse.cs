using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Infrastructure.Clients;

internal sealed record MovimientoDetalleResponse(
    Guid IdMovimiento,
    Guid IdCuenta,
    Guid IdCuentaCategoria,
    string? NombreCategoria,
    TipoMovimiento TipoMovimiento,
    string Concepto,
    string? Establecimiento,
    decimal Importe,
    string Moneda,
    string? IdComprobante,
    string Nota,
    DateTime FechaMovimiento);

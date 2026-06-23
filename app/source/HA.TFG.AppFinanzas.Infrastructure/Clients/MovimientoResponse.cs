using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Infrastructure.Clients;

internal sealed record MovimientoResponse(
    Guid IdMovimiento,
    Guid IdCuenta,
    Guid? IdCategoria,
    string? NombreCategoria,
    TipoMovimiento TipoMovimiento,
    string Concepto,
    decimal Importe,
    string Moneda,
    DateOnly FechaMovimiento);

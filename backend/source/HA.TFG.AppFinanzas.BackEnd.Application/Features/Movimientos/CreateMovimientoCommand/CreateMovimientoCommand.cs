using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

public record CreateMovimientoCommand : IRequest<CreateMovimientoResult>
{
    public string Email { get; init; } = string.Empty;
    public Guid IdCuenta { get; init; }
    public Guid? IdCuentaCategoria { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
    public required string Concepto { get; init; }
    public decimal Importe { get; init; }
    public required string Moneda { get; init; }
    public decimal? TipoCambio { get; init; }
    public string Nota { get; init; } = string.Empty;
    public DateTime FechaMovimiento { get; init; }

    // Datos opcionales del comprobante adjunto
    public Stream? ComprobanteStream { get; init; }
    public string? ComprobanteFileName { get; init; }
    public string? ComprobanteContentType { get; init; }
}

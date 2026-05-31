using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

/// <summary>
/// Procesa un comprobante de forma completa: extrae texto con Document Intelligence,
/// obtiene las categorías de la cuenta desde la base de datos, construye el prompt
/// y lo envía a Foundry AI para obtener un JSON listo para crear un movimiento.
/// </summary>
public record ProcesarComprobanteQuery : IRequest<ComprobanteExtraidoDto>
{
    public required Stream ComprobanteStream { get; init; }
    public required string ContentType { get; init; }
    public required Guid IdCuenta { get; init; }
    public required string Email { get; init; }
}

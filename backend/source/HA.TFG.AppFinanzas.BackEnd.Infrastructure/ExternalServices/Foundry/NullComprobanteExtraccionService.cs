using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

/// <summary>
/// Implementación nula del servicio de extracción de datos vía LLM.
/// Se usa cuando el proveedor no está configurado o no está disponible.
/// </summary>
internal sealed class NullComprobanteExtraccionService : IComprobanteExtraccionService
{
    public Task<string?> ExtractDatosAsync(string textoComprobante, CancellationToken cancellationToken)
        => Task.FromResult<string?>(null);
}

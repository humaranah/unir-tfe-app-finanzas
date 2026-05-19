using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

/// <summary>
/// Implementación no-op de <see cref="IComprobanteStorageService"/> que se registra
/// cuando la configuración de almacenamiento es inválida o desconocida.
/// </summary>
internal sealed class NullComprobanteStorageService : IComprobanteStorageService
{
    public Task<string> UploadComprobanteAsync(
        Guid idCuenta, string fileName, string contentType, Stream stream, CancellationToken cancellationToken)
        => Task.FromResult(string.Empty);

    public Task<ComprobanteFile> GetComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
        => throw new InvalidOperationException("El almacenamiento de comprobantes no está configurado.");

    public Task DeleteComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

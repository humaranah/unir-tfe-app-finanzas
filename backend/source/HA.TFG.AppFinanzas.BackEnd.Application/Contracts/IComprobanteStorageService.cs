namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IComprobanteStorageService
{
    /// <summary>
    /// Sube un comprobante al almacenamiento configurado y devuelve su identificador único.
    /// </summary>
    /// <param name="idCuenta">Identificador de la cuenta, usado como prefijo de carpeta.</param>
    /// <param name="fileName">Nombre original del archivo.</param>
    /// <param name="contentType">Tipo MIME del archivo.</param>
    /// <param name="stream">Contenido del archivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Identificador único del comprobante almacenado.</returns>
    Task<string> UploadComprobanteAsync(
        Guid idCuenta,
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Elimina un comprobante previamente subido. Usado para rollback si falla la BD.
    /// </summary>
    Task DeleteComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken);
}

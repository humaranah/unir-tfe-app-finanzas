using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

internal sealed class LocalComprobanteStorageService(
    IOptions<ComprobanteStorageConfig> options,
    ILogger<LocalComprobanteStorageService> logger) : IComprobanteStorageService
{
    public async Task<string> UploadComprobanteAsync(
        Guid idCuenta,
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var basePath = string.IsNullOrWhiteSpace(options.Value.LocalBasePath)
            ? Path.Combine(Path.GetTempPath(), "comprobantes")
            : options.Value.LocalBasePath;
        var carpetaCuenta = Path.Combine(basePath, idCuenta.ToString());
        Directory.CreateDirectory(carpetaCuenta);

        var extension = Path.GetExtension(fileName);
        var idComprobante = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpetaCuenta, idComprobante);

        await using var fileStream = File.Create(rutaCompleta);
        await stream.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Comprobante guardado localmente: {RutaCompleta}", rutaCompleta);

        return idComprobante;
    }

    public Task DeleteComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
    {
        var basePath = options.Value.LocalBasePath ?? Path.Combine(Path.GetTempPath(), "comprobantes");
        var rutaCompleta = Path.Combine(basePath, idCuenta.ToString(), idComprobante);

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);

        logger.LogInformation("Comprobante eliminado localmente: {RutaCompleta}", rutaCompleta);

        return Task.CompletedTask;
    }
}

using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

internal sealed class LocalComprobanteStorageService(
    IOptions<ComprobanteStorageConfig> options,
    ILogger<LocalComprobanteStorageService> logger) : IComprobanteStorageService
{
    private string BasePath => string.IsNullOrWhiteSpace(options.Value.LocalBasePath)
        ? Path.Combine(Path.GetTempPath(), "comprobantes")
        : options.Value.LocalBasePath;

    public async Task<string> UploadComprobanteAsync(
        Guid idCuenta,
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var carpetaCuenta = Path.Combine(BasePath, idCuenta.ToString());
        Directory.CreateDirectory(carpetaCuenta);

        var extension = Path.GetExtension(fileName);
        var idComprobante = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpetaCuenta, idComprobante);

        await using var fileStream = File.Create(rutaCompleta);
        await stream.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Comprobante guardado localmente: {RutaCompleta}", rutaCompleta);

        return idComprobante;
    }

    public Task<ComprobanteFile> GetComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
    {
        var rutaCompleta = Path.Combine(BasePath, idCuenta.ToString(), idComprobante);

        if (!File.Exists(rutaCompleta))
            throw new FileNotFoundException($"Comprobante no encontrado en la ruta local.", rutaCompleta);

        var contentType = Path.GetExtension(idComprobante).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var stream = File.OpenRead(rutaCompleta);
        return Task.FromResult(new ComprobanteFile(stream, contentType, idComprobante));
    }

    public Task DeleteComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
    {
        var rutaCompleta = Path.Combine(BasePath, idCuenta.ToString(), idComprobante);

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);

        logger.LogInformation("Comprobante eliminado localmente: {RutaCompleta}", rutaCompleta);

        return Task.CompletedTask;
    }
}

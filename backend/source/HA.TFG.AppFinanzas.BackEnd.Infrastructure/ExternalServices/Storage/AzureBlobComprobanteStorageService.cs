using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

internal sealed class AzureBlobComprobanteStorageService(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobComprobanteStorageService> logger) : IComprobanteStorageService
{
    private const string ContainerName = "comprobantes";

    public async Task<string> UploadComprobanteAsync(
        Guid idCuenta,
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var extension = Path.GetExtension(fileName);
        var idComprobante = $"{Guid.NewGuid()}{extension}";
        var blobName = $"{idCuenta}/{idComprobante}";

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        logger.LogInformation("Comprobante subido a Azure Blob: {BlobName}", blobName);

        return idComprobante;
    }

    public async Task<ComprobanteFile> GetComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
    {
        var blobName = $"{idCuenta}/{idComprobante}";
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var contentType = properties.Value.ContentType ?? "application/octet-stream";
        var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);

        return new ComprobanteFile(stream, contentType, idComprobante);
    }

    public async Task DeleteComprobanteAsync(Guid idCuenta, string idComprobante, CancellationToken cancellationToken)
    {
        var blobName = $"{idCuenta}/{idComprobante}";
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Comprobante eliminado de Azure Blob: {BlobName}", blobName);
    }
}

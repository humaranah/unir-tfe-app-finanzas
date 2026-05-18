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
        var blobName = $"{idCuenta}/{Guid.NewGuid()}{extension}";

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        logger.LogInformation("Comprobante subido a Azure Blob: {BlobName}", blobName);

        return blobName;
    }

    public async Task DeleteComprobanteAsync(string idComprobante, CancellationToken cancellationToken)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(idComprobante);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Comprobante eliminado de Azure Blob: {BlobName}", idComprobante);
    }
}

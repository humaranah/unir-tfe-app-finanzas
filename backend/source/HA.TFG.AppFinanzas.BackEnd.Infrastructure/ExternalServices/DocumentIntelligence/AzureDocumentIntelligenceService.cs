using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

internal sealed class AzureDocumentIntelligenceService(
    DocumentAnalysisClient client,
    IOptions<DocumentIntelligenceConfig> options,
    ILogger<AzureDocumentIntelligenceService> logger)
    : IComprobanteAnalysisService
{
    private readonly DocumentIntelligenceConfig _config = options.Value;

    public async Task<ComprobanteAnalysisResult?> AnalyzeAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _config.ModelId,
                stream,
                cancellationToken: cancellationToken);

            var result = operation.Value;

            var sb = new StringBuilder();
            foreach (var page in result.Pages)
            {
                if (sb.Length > 0) sb.AppendLine(); // separador entre páginas

                DocumentLayoutReconstructor.ReconstruirPagina(page, sb);
            }

            return new ComprobanteAnalysisResult { Texto = sb.ToString().TrimEnd() };
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex,
                "Error al llamar a Azure Document Intelligence. Código: {StatusCode}", ex.Status);
            return null;
        }
    }
}


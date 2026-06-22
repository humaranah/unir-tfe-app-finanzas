using Azure;
using Azure.AI.DocumentIntelligence;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

internal sealed class AzureDocumentIntelligenceService(
    DocumentIntelligenceClient client,
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
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);

            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _config.ModelId,
                BinaryData.FromBytes(ms.ToArray()),
                cancellationToken);

            var result = operation.Value;

            var sb = new StringBuilder();
            foreach (var page in result.Pages ?? [])
            {
                if (sb.Length > 0) sb.AppendLine();
                DocumentLayoutReconstructor.ReconstruirPagina(page, sb);
            }

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("DI: modelo={ModelId}, páginas={Pages}, documentos={Docs}",
                    _config.ModelId, result.Pages?.Count ?? 0, result.Documents?.Count ?? 0);

            DocumentFieldDictionary? fields = result.Documents?.Count > 0
                ? result.Documents[0].Fields
                : null;

            if (fields is null)
            {
                logger.LogWarning("Document Intelligence no reconoció un comprobante en el documento analizado.");
            }
            else if (logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var kv in fields)
                    logger.LogDebug("DI campo: {Key} | tipo={Type} | contenido={Content}",
                        kv.Key, kv.Value.FieldType, kv.Value.Content);
            }

            var analysisResult = new ComprobanteAnalysisResult
            {
                Texto = sb.ToString().TrimEnd(),
                MerchantName = ExtractString(fields, "MerchantName"),
                CountryRegion = ExtractCountryRegion(fields),
                TransactionDate = ExtractDate(fields),
                TransactionTime = ExtractTime(fields),
                Total = ExtractCurrencyAmount(fields, "Total"),
                Currency = ExtractCurrencyCode(fields, "Total"),
                Items = ExtractItems(fields),
            };

            logger.LogDebug("Documento analizado: {@AnalisisResult}",
                analysisResult with { Texto = $"Texto con {analysisResult.Texto.Length} caracteres" });

            return analysisResult;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex,
                "Error al llamar a Azure Document Intelligence. Código: {StatusCode}", ex.Status);
            return null;
        }
    }

    // ── Helpers de extracción ─────────────────────────────────────────────────

    private static string? ExtractString(DocumentFieldDictionary? fields, string name)
    {
        if (fields is null || !fields.TryGetValue(name, out var f) || f.FieldType != DocumentFieldType.String)
            return null;
        return f.ValueString;
    }

    private static string? ExtractCountryRegion(DocumentFieldDictionary? fields)
    {
        if (fields is null || !fields.TryGetValue("CountryRegion", out var f)) return null;
        if (f.FieldType == DocumentFieldType.CountryRegion) return f.ValueCountryRegion;
        if (f.FieldType == DocumentFieldType.String) return f.ValueString;
        return null;
    }

    private static DateOnly? ExtractDate(DocumentFieldDictionary? fields)
    {
        if (fields is null || !fields.TryGetValue("TransactionDate", out var f) || f.FieldType != DocumentFieldType.Date)
            return null;
        return f.ValueDate is { } d ? DateOnly.FromDateTime(d.Date) : null;
    }

    private static TimeOnly? ExtractTime(DocumentFieldDictionary? fields)
    {
        if (fields is null || !fields.TryGetValue("TransactionTime", out var f) || f.FieldType != DocumentFieldType.Time)
            return null;
        return f.ValueTime is { } t ? TimeOnly.FromTimeSpan(t) : null;
    }

    private static decimal? ExtractCurrencyAmount(DocumentFieldDictionary? fields, string name)
    {
        if (fields is null || !fields.TryGetValue(name, out var f) || f.FieldType != DocumentFieldType.Currency)
            return null;
        return f.ValueCurrency is { } cv ? (decimal)cv.Amount : null;
    }

    private static string? ExtractCurrencyCode(DocumentFieldDictionary? fields, string name)
    {
        if (fields is null || !fields.TryGetValue(name, out var f) || f.FieldType != DocumentFieldType.Currency)
            return null;
        return f.ValueCurrency?.CurrencyCode;
    }

    private static IReadOnlyList<ReceiptItemResult> ExtractItems(DocumentFieldDictionary? fields)
    {
        if (fields is null || !fields.TryGetValue("Items", out var f) || f.FieldType != DocumentFieldType.List)
            return [];

        var items = new List<ReceiptItemResult>();
        foreach (var element in f.ValueList ?? [])
        {
            if (element.FieldType != DocumentFieldType.Dictionary) continue;
            var d = element.ValueDictionary;
            if (d is null) continue;
            items.Add(new ReceiptItemResult
            {
                Description = ExtractString(d, "Description"),
                Quantity = ExtractDouble(d, "Quantity"),
                Price = ExtractCurrencyAmount(d, "Price"),
                TotalPrice = ExtractCurrencyAmount(d, "TotalPrice"),
            });
        }
        return items;
    }

    private static decimal? ExtractDouble(DocumentFieldDictionary? fields, string name)
    {
        if (fields is null || !fields.TryGetValue(name, out var f) || f.FieldType != DocumentFieldType.Double)
            return null;
        return f.ValueDouble is { } n ? (decimal)n : null;
    }
}

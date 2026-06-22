using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Mediator;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

public class ProcesarComprobanteQueryHandler(
    IComprobanteAnalysisService analysisService,
    ILlmService extraccionService,
    IUsuarioRepository usuarioRepository,
    ICuentaCategoriaRepository cuentaCategoriaRepository)
    : IRequestHandler<ProcesarComprobanteQuery, ComprobanteExtraidoDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public async ValueTask<ComprobanteExtraidoDto> Handle(
        ProcesarComprobanteQuery request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Extraer texto e información estructurada con Document Intelligence (prebuilt-receipt)
        var analysisResult = await analysisService.AnalyzeAsync(
            request.ComprobanteStream,
            request.ContentType,
            cancellationToken) ?? throw new ExternalServiceException(
                "DocumentIntelligence",
                "No se pudo extraer texto del comprobante. Comprueba la configuración del servicio o el archivo enviado.");

        if (!analysisResult.HasStructuredData && string.IsNullOrWhiteSpace(analysisResult.Texto))
            throw new ExternalServiceException(
                "DocumentIntelligence",
                "No se pudo extraer texto del comprobante. Comprueba la configuración del servicio o el archivo enviado.");

        // Paso 2: Obtener las categorías de la cuenta desde la base de datos
        var usuario = await usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.Email);

        IReadOnlyList<CuentaCategoria> categorias;
        try
        {
            categorias = await cuentaCategoriaRepository.GetCategoriasByCuentaAsync(
                usuario.IdUsuario, request.IdCuenta, cancellationToken);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "BaseDeDatos",
                "Error al obtener las categorías de la cuenta desde la base de datos.",
                ex);
        }

        if (categorias.Count == 0)
            throw new ExternalServiceException(
                "BaseDeDatos",
                "No hay categorías disponibles para la cuenta. No es posible procesar el comprobante.");

        // Paso 3: Construir el prompt con los datos DI confirmados + texto OCR
        var prompt = ComprobantePromptBuilder.Build(analysisResult, categorias);

        // Paso 4: Enviar el prompt al LLM
        var respuesta = await extraccionService.EnviarPromptAsync(prompt, cancellationToken);

        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ExternalServiceException(
                "FoundryAI",
                "El modelo de IA no devolvió una respuesta válida. Inténtalo de nuevo más tarde.");

        // Paso 5: Limpiar posible bloque markdown y deserializar
        var json = MarkdownJsonExtractor.Extract(respuesta);

        ComprobanteExtraidoDto llmResult;
        try
        {
            llmResult = JsonSerializer.Deserialize<ComprobanteExtraidoDto>(json, JsonOptions)
                ?? throw new ExternalServiceException(
                    "FoundryAI",
                    "La respuesta del modelo no pudo deserializarse a un comprobante válido.");
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "FoundryAI",
                $"La respuesta del modelo no es un JSON válido: {ex.Message}");
        }

        // Paso 6: Fusionar: los campos extraídos por DI tienen prioridad; el LLM cubre el resto
        return MergeResults(analysisResult, llmResult);
    }

    private static ComprobanteExtraidoDto MergeResults(ComprobanteAnalysisResult di, ComprobanteExtraidoDto llm) =>
        new()
        {
            Establecimiento = di.MerchantName ?? llm.Establecimiento,
            Importe = di.Total ?? llm.Importe,
            FechaMovimiento = di.FechaMovimiento ?? llm.FechaMovimiento,
            Moneda = di.Currency ?? llm.Moneda,
            Concepto = di.Concepto ?? llm.Concepto,
            TipoMovimiento = llm.TipoMovimiento,
            IdCuentaCategoria = llm.IdCuentaCategoria,
            CategoriaPropuesta = llm.CategoriaPropuesta,
            Nota = llm.Nota,
        };
}

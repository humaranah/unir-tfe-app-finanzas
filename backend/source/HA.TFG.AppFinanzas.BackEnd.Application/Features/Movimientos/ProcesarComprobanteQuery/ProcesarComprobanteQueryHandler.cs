using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Mediator;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

public class ProcesarComprobanteQueryHandler(
    IComprobanteAnalysisService analysisService,
    IComprobanteExtraccionService extraccionService,
    ICategoriaRepository categoriaRepository)
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
        // Paso 1: Extraer texto del comprobante con Document Intelligence
        var analysisResult = await analysisService.AnalyzeAsync(
            request.ComprobanteStream,
            request.ContentType,
            cancellationToken);

        if (analysisResult is null || string.IsNullOrWhiteSpace(analysisResult.Texto))
            throw new ExternalServiceException(
                "DocumentIntelligence",
                "No se pudo extraer texto del comprobante. Comprueba la configuración del servicio o el archivo enviado.");

        // Paso 2: Obtener categorías desde la base de datos
        IReadOnlyList<Categoria> categorias;
        try
        {
            categorias = await categoriaRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "BaseDeDatos",
                "Error al obtener las categorías desde la base de datos.",
                ex);
        }

        if (categorias.Count == 0)
            throw new ExternalServiceException(
                "BaseDeDatos",
                "No hay categorías disponibles en la base de datos. No es posible procesar el comprobante.");

        // Paso 3: Construir el prompt optimizado
        var prompt = BuildPrompt(analysisResult.Texto, categorias);

        // Paso 4: Enviar el prompt a Foundry AI (gpt-4o-mini)
        var respuesta = await extraccionService.EnviarPromptAsync(prompt, cancellationToken);

        // Paso 5: Limpiar posible bloque markdown y deserializar a DTO
        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ExternalServiceException(
                "FoundryAI",
                "El modelo de IA no devolvió una respuesta válida. Inténtalo de nuevo más tarde.");

        var json = ExtractJson(respuesta);

        try
        {
            return JsonSerializer.Deserialize<ComprobanteExtraidoDto>(json, JsonOptions)
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
    }

    private static string ExtractJson(string respuesta)
    {
        var texto = respuesta.Trim();
        if (texto.StartsWith("```", StringComparison.Ordinal))
        {
            var inicio = texto.IndexOf('\n');
            var fin = texto.LastIndexOf("```", StringComparison.Ordinal);
            if (inicio >= 0 && fin > inicio)
                return texto[(inicio + 1)..fin].Trim();
        }
        return texto;
    }

    private static string BuildPrompt(string textoComprobante, IReadOnlyList<Categoria> categorias)
    {
        var sbCategorias = new StringBuilder();
        foreach (var cat in categorias)
            sbCategorias.AppendLine($"- id={cat.IdCategoria} | tipo={cat.TipoMovimiento} | nombre={cat.Nombre}");

        return $$"""
            Extrae datos del comprobante y devuelve SOLO este JSON:
            {
              "establecimiento": "<string|null>",
              "concepto": "<string|null>",
              "importe": <number|null>,
              "moneda": "<string|null>",
              "fechaMovimiento": "<YYYY-MM-DDTHH:mm:ss±HH:MM|null>",
              "tipoMovimiento": "<Ingreso|Gasto|Transferencia|null>",
              "idCuentaCategoria": "<uuid|null>",
              "nota": "<string|null>"
            }

            REGLAS:
            - establecimiento: nombre del comercio o razón social.
            - concepto: descripción principal del producto/servicio.
            - importe: total pagado.
            - moneda: si no aparece, usa la moneda local del país del comprobante.
            - fechaMovimiento: usar fecha y hora del ticket en ISO 8601 con TZ; si no hay hora, usar T00:00:00.
            - tipoMovimiento: compras->{{TipoMovimiento.Gasto}}; ingresos->{{TipoMovimiento.Ingreso}}; entre cuentas->{{TipoMovimiento.Transferencia}}.
            - idCuentaCategoria: elegir la categoría más relacionada; si no es claro, usar "c73ebca7-2bf5-fd6e-e041-642b86a9aa02".
            - nota: info útil como método de pago o tienda.

            CATEGORÍAS:
            {{sbCategorias.ToString().TrimEnd()}}

            TEXTO DEL COMPROBANTE:
            {{textoComprobante}}
            """;
    }
}

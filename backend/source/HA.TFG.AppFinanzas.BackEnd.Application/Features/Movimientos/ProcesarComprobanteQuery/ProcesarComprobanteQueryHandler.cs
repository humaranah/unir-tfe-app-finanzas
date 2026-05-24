using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Mediator;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

public class ProcesarComprobanteQueryHandler(
    IComprobanteAnalysisService analysisService,
    IComprobanteExtraccionService extraccionService,
    ICategoriaRepository categoriaRepository)
    : IRequestHandler<ProcesarComprobanteQuery, string>
{
    public async ValueTask<string> Handle(
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
        var prompt = ConstruirPrompt(analysisResult.Texto, categorias);

        // Paso 4: Enviar el prompt a Foundry AI (gpt-4o-mini)
        var respuesta = await extraccionService.EnviarPromptAsync(prompt, cancellationToken);

        // Paso 5: Devolver la respuesta
        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ExternalServiceException(
                "FoundryAI",
                "El modelo de IA no devolvió una respuesta válida. Inténtalo de nuevo más tarde.");

        return respuesta;
    }

    private static string ConstruirPrompt(string textoComprobante, IReadOnlyList<Categoria> categorias)
    {
        var tiposMovimiento = Enum.GetNames<TipoMovimiento>();

        var sbCategorias = new StringBuilder();
        foreach (var cat in categorias)
            sbCategorias.AppendLine($"- id={cat.IdCategoria} | tipo={cat.TipoMovimiento} | nombre={cat.Nombre}");

        return $$"""
            Eres un asistente especializado en analizar tickets y facturas.
            A partir del texto del comprobante, genera un JSON con los campos necesarios para registrar un movimiento financiero.

            TIPOS DE MOVIMIENTO VÁLIDOS: {{string.Join(", ", tiposMovimiento)}}

            CATEGORÍAS DISPONIBLES:
            {{sbCategorias.ToString().TrimEnd()}}

            TEXTO DEL COMPROBANTE:
            {{textoComprobante}}

            Devuelve ÚNICAMENTE un JSON con esta estructura (usa null si no encuentras el dato):
            {
              "concepto": "<string o null>",
              "importe": "<number o null>",
              "moneda": "<string o null>",
              "fechaMovimiento": "<YYYY-MM-DD o null>",
              "tipoMovimiento": "<{{string.Join("|", tiposMovimiento)}} o null>",
              "idCuentaCategoria": "<uuid de la categoría más adecuada o null>",
              "nota": "<string o null>"
            }
            """;
    }
}

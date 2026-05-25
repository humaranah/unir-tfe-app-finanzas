namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

/// <summary>
/// Elimina el bloque de código markdown (```json ... ```) si el LLM lo incluye en su respuesta.
/// </summary>
internal static class MarkdownJsonExtractor
{
    internal static string Extract(string respuesta)
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
}

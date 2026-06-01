namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;

public record RecomendacionResult
{
    /// <summary>Texto generado por el LLM con el análisis y las recomendaciones.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Momento en que se generó la recomendación.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

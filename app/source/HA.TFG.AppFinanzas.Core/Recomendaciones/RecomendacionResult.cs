namespace HA.TFG.AppFinanzas.Core.Recomendaciones;

public record RecomendacionResult
{
    public required string Content { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

public sealed class DocumentIntelligenceConfig
{
    public const string SectionName = "DocumentIntelligence";

    /// <summary>Proveedor activo: "Azure" o "Null".</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>Endpoint del recurso Azure Document Intelligence (solo si Provider = "Azure").</summary>
    public string? Endpoint { get; init; }

    /// <summary>Clave de API del recurso Azure Document Intelligence (solo si Provider = "Azure").</summary>
    public string? ApiKey { get; init; }

    /// <summary>Identificador del modelo a usar. Por defecto "prebuilt-layout".</summary>
    public string ModelId { get; init; } = "prebuilt-layout";
}

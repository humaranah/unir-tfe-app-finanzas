namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

public sealed class FoundryConfig
{
    public const string SectionName = "Foundry";

    /// <summary>Proveedor activo: "Azure" o "Null".</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Endpoint del proyecto Azure AI Foundry.
    /// Formato: https://&lt;resource&gt;.services.ai.azure.com/api/projects/&lt;project&gt;
    /// </summary>
    public string? ProjectEndpoint { get; init; }

    /// <summary>Nombre del despliegue del modelo (ej. "gpt-4o-mini").</summary>
    public string DeploymentName { get; init; } = "gpt-4o-mini";

    /// <summary>Instrucciones de sistema que se envían al LLM.</summary>
    public string SystemPrompt { get; init; } =
        """
        Eres un asistente especializado en analizar tickets y facturas.
        A partir del texto que te proporcione el usuario, extrae en JSON los siguientes campos
        (usa null si no encuentras el dato):
        {
          "nombreComercio": string | null,
          "fechaTransaccion": "YYYY-MM-DD" | null,
          "importeTotal": number | null,
          "moneda": string | null,
          "conceptos": [{ "descripcion": string, "importe": number | null }]
        }
        Devuelve únicamente el JSON, sin explicaciones adicionales.
        """;
}

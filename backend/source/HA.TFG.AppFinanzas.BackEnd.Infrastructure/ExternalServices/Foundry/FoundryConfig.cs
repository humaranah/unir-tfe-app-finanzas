namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Foundry;

public sealed class FoundryConfig
{
    public const string SectionName = "Foundry";

    /// <summary>
    /// Endpoint del proyecto Azure AI Foundry.
    /// Formato: https://&lt;resource&gt;.services.ai.azure.com/api/projects/&lt;project&gt;
    /// </summary>
    public string? ProjectEndpoint { get; init; }

    /// <summary>Nombre del despliegue del modelo (ej. "gpt-4o-mini").</summary>
    public string DeploymentName { get; init; } = "gpt-4o-mini";

    /// <summary>Instrucciones de sistema que se envían al LLM.</summary>
    public string Instructions { get; init; } =
        """
        Eres un asistente especializado en analizar tickets y facturas.
        Sigues estrictamente las instrucciones del usuario y devuelves únicamente JSON estructurado, sin texto adicional ni bloques markdown.
        Si algún dato no puede determinarse, usa null cuando el esquema lo permita.
        Respeta exactamente los nombres de propiedades y la estructura del contrato solicitado por la aplicación.
        """;
}

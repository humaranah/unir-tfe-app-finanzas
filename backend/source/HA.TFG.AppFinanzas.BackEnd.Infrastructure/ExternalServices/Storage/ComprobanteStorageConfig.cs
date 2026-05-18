using System.ComponentModel.DataAnnotations;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

public sealed class ComprobanteStorageConfig
{
    public const string SectionName = "ComprobanteStorage";

    /// <summary>Proveedor activo: "Azure" o "Local".</summary>
    [Required]
    public string Provider { get; init; } = "Local";

    /// <summary>Cadena de conexión de Azure Storage (solo si Provider = "Azure").</summary>
    public string? AzureConnectionString { get; init; }

    /// <summary>Ruta base para almacenamiento local (solo si Provider = "Local").</summary>
    public string? LocalBasePath { get; init; }
}

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Storage;

public enum ComprobanteStorageProvider { Disabled, Azure, Local }

public sealed class ComprobanteStorageConfig
{
    public const string SectionName = "ComprobanteStorage";

    /// <summary>Proveedor activo.</summary>
    public ComprobanteStorageProvider Provider { get; init; } = ComprobanteStorageProvider.Disabled;

    /// <summary>Cadena de conexión de Azure Storage (solo si Provider = "Azure").</summary>
    public string? AzureConnectionString { get; init; }

    /// <summary>Ruta base para almacenamiento local (solo si Provider = "Local").</summary>
    public string? LocalBasePath { get; init; }
}

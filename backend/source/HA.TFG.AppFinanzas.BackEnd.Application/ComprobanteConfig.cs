namespace HA.TFG.AppFinanzas.BackEnd.Application;

public sealed class ComprobanteConfig
{
    public const string SectionName = "Comprobante";

    /// <summary>Tamaño máximo permitido para archivos de comprobante, en bytes. Por defecto 1 MB.</summary>
    public long MaxSizeBytes { get; init; } = 1 * 1024 * 1024;
}

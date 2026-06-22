namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

/// <summary>
/// Resultado del análisis de un comprobante mediante Document Intelligence (prebuilt-receipt).
/// Combina los campos estructurados extraídos por el modelo con el texto OCR en bruto.
/// </summary>
public sealed record ComprobanteAnalysisResult
{
    /// <summary>Texto completo reconstruido del comprobante (contexto adicional para el LLM).</summary>
    public string Texto { get; init; } = string.Empty;

    // ── Campos estructurados del modelo prebuilt-receipt ──────────────────────

    /// <summary>Nombre del comercio o razón social.</summary>
    public string? MerchantName { get; init; }

    /// <summary>Código de país/región en formato ISO 3166-1 alpha-2 (ej: "PE", "ES").</summary>
    public string? CountryRegion { get; init; }

    /// <summary>Fecha de la transacción.</summary>
    public DateOnly? TransactionDate { get; init; }

    /// <summary>Hora de la transacción.</summary>
    public TimeOnly? TransactionTime { get; init; }

    /// <summary>
    /// Fecha y hora combinadas de la transacción en UTC.
    /// Null cuando <see cref="TransactionDate"/> no fue extraída.
    /// Si solo se extrajo fecha, la hora se establece en medianoche.
    /// </summary>
    public DateTimeOffset? FechaMovimiento => TransactionDate.HasValue
        ? new DateTimeOffset(TransactionDate.Value.ToDateTime(TransactionTime ?? TimeOnly.MinValue), TimeSpan.Zero)
        : null;

    /// <summary>Importe total de la transacción.</summary>
    public decimal? Total { get; init; }

    /// <summary>Código de moneda ISO 4217 asociado al total (ej: "PEN", "EUR").</summary>
    public string? Currency { get; init; }

    /// <summary>Artículos detallados del comprobante.</summary>
    public IReadOnlyList<ReceiptItemResult> Items { get; init; } = [];

    /// <summary>
    /// Descripción directa cuando hay exactamente un artículo con descripción conocida.
    /// Null en cualquier otro caso; el LLM debe inferirlo.
    /// </summary>
    public string? Concepto => Items.Count == 1 && !string.IsNullOrWhiteSpace(Items[0].Description)
        ? Items[0].Description
        : null;

    /// <summary>
    /// Indica si se extrajeron datos estructurados del comprobante (MerchantName, Total, etc.).
    /// </summary>
    public bool HasStructuredData =>
        MerchantName is not null ||
        Total is not null ||
        TransactionDate is not null ||
        Currency is not null ||
        CountryRegion is not null ||
        Items.Count > 0;
}

/// <summary>Artículo individual extraído del comprobante por Document Intelligence.</summary>
public sealed record ReceiptItemResult
{
    public string? Description { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? Price { get; init; }
    public decimal? TotalPrice { get; init; }
}

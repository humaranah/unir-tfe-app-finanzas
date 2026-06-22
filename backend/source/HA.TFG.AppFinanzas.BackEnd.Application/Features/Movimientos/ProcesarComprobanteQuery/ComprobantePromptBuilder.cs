using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using System.Globalization;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

internal static class ComprobantePromptBuilder
{
    private static readonly Guid IdCategoriaOtrosGastos = new("c73ebca7-2bf5-fd6e-e041-642b86a9aa02");

    internal static string Build(ComprobanteAnalysisResult di, IReadOnlyList<CuentaCategoria> categorias)
    {
        var idDefault = (categorias.FirstOrDefault(c => c.IdCategoria == IdCategoriaOtrosGastos)
            ?? categorias[0]).IdCuentaCategoria;

        var sbCategorias = new StringBuilder();
        foreach (var cat in categorias)
            sbCategorias.AppendLine($"- id={cat.IdCuentaCategoria} | tipo={cat.TipoMovimiento} | nombre={cat.Nombre}");

        var schema = BuildJsonSchema(di);
        var diBlock = BuildDiSection(di);
        var reglas = BuildReglas(di, idDefault);
        var texto = string.IsNullOrWhiteSpace(di.Texto)
            ? string.Empty
            : $"\nTEXTO DEL COMPROBANTE:\n{di.Texto}";

        return $"""
            Devuelve SOLO este JSON con los datos del comprobante:
            {schema}

            DATOS CONFIRMADOS (Document Intelligence):
            {diBlock.ToString().TrimEnd()}

            INSTRUCCIONES:
            {reglas.ToString().TrimEnd()}

            CATEGORÍAS:
            {sbCategorias.ToString().TrimEnd()}{texto}
            """;
    }

    /// <summary>
    /// Schema JSON dinámico: solo incluye los campos que el LLM debe inferir.
    /// Los campos que DI ya extrajo de forma fiable se omiten para reducir tokens.
    /// </summary>
    private static string BuildJsonSchema(ComprobanteAnalysisResult di)
    {
        var fields = new List<string>();

        if (di.MerchantName is null) fields.Add("  \"establecimiento\": \"<string|null>\"");
        if (di.Concepto is null) fields.Add("  \"concepto\": \"<string|null>\"");
        if (di.Total is null) fields.Add("  \"importe\": <number|null>");
        if (di.Currency is null) fields.Add("  \"moneda\": \"<string|null>\"");
        if (di.TransactionDate is null) fields.Add("  \"fechaMovimiento\": \"<YYYY-MM-DDTHH:mm:ss±HH:MM|null>\"");
        fields.Add("  \"tipoMovimiento\": \"<Ingreso|Gasto|Transferencia|null>\"");
        fields.Add("  \"idCuentaCategoria\": \"<uuid|null>\"");
        fields.Add("  \"categoriaPropuesta\": \"<string|null>\"");
        fields.Add("  \"nota\": \"<string|null>\"");

        return "{\n" + string.Join(",\n", fields) + "\n}";
    }

    /// <summary>
    /// Sección de datos DI: solo muestra los campos que se extrajeron (sin listar nulos).
    /// </summary>
    private static StringBuilder BuildDiSection(ComprobanteAnalysisResult di)
    {
        var sb = new StringBuilder();

        if (di.MerchantName is not null)
            sb.AppendLine($"- Comercio: {di.MerchantName}");

        if (di.TransactionDate.HasValue)
        {
            var hora = di.TransactionTime.HasValue ? $" {di.TransactionTime.Value:HH:mm:ss}" : "";
            sb.AppendLine($"- Fecha/Hora: {di.TransactionDate.Value:yyyy-MM-dd}{hora}");
        }

        if (di.Total.HasValue)
            sb.AppendLine($"- Total: {di.Total.Value.ToString("F2", CultureInfo.InvariantCulture)}");

        if (di.Currency is not null)
            sb.AppendLine($"- Moneda: {di.Currency}");

        if (di.CountryRegion is not null)
            sb.AppendLine($"- País/Región: {di.CountryRegion}");

        if (di.Concepto is not null)
            sb.AppendLine($"- Concepto: {di.Concepto}");

        if (di.Items.Count > 0)
        {
            sb.AppendLine("- Artículos:");
            foreach (var item in di.Items)
            {
                var qty = item.Quantity.HasValue ? $" × {item.Quantity.Value.ToString("G", CultureInfo.InvariantCulture)}" : "";
                var precio = item.TotalPrice ?? item.Price;
                var pStr = precio.HasValue ? $" — {precio.Value.ToString("F2", CultureInfo.InvariantCulture)}" : "";
                sb.AppendLine($"  • {item.Description ?? "?"}{qty}{pStr}");
            }
        }

        if (sb.Length == 0)
            sb.AppendLine("(ninguno extraído; inferir todo del texto del comprobante)");

        return sb;
    }

    /// <summary>
    /// Instrucciones del prompt: solo para los campos que el LLM debe rellenar.
    /// Inyecta contexto DI en las reglas de clasificación para mejorar la precisión.
    /// </summary>
    private static StringBuilder BuildReglas(ComprobanteAnalysisResult di, Guid idDefault)
    {
        var sb = new StringBuilder();

        if (di.MerchantName is null)
            sb.AppendLine("- establecimiento: nombre del comercio o razón social.");

        if (di.Concepto is null)
        {
            var conceptoHint = di.Items.Count > 0
                ? "resumir los artículos DI"
                : "extraer del texto del comprobante";
            sb.AppendLine($"- concepto: descripción principal del gasto ({conceptoHint}).");
        }

        if (di.Total is null)
            sb.AppendLine("- importe: total pagado.");

        if (di.Currency is null)
        {
            var regionHint = di.CountryRegion is not null ? $" del país {di.CountryRegion}" : "";
            sb.AppendLine($"- moneda: código ISO 4217; derivar{regionHint} o del texto del comprobante.");
        }

        if (di.TransactionDate is null)
            sb.AppendLine("- fechaMovimiento: ISO 8601 con zona horaria; si no hay hora → T00:00:00.");

        sb.AppendLine($"- tipoMovimiento: compras→{TipoMovimiento.Gasto}; ingresos→{TipoMovimiento.Ingreso}; entre cuentas→{TipoMovimiento.Transferencia}.");

        var ctxCategoria = di.MerchantName is not null ? $" para el comercio \"{di.MerchantName}\"" : "";
        sb.AppendLine($"- idCuentaCategoria: categoría más relacionada{ctxCategoria}; si no es claro, usar \"{idDefault}\".");
        sb.AppendLine($"- categoriaPropuesta: si idCuentaCategoria es \"{idDefault}\", sugerir nombre; si no, null.");
        sb.AppendLine("- nota: método de pago, tienda u otro detalle útil, expresivo y conciso.");

        return sb;
    }
}

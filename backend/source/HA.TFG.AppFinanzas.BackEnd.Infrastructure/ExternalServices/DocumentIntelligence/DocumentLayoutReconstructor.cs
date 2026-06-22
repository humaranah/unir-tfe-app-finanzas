using Azure.AI.DocumentIntelligence;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

/// <summary>
/// Reconstruye el texto de una página agrupando líneas por posición vertical y horizontal.
/// </summary>
internal static class DocumentLayoutReconstructor
{
    /// <summary>Línea con coordenadas relativas (0..1) respecto al tamaño de la página.</summary>
    internal record struct ProjectedLine(string Content, double XRel, double YRel);

    /// <summary>Separador usado entre columnas de una misma fila visual.</summary>
    internal const string Separator = " | ";

    // Factores aplicados sobre el alto promedio de línea para calcular los umbrales dinámicos.
    // ThresholdRowFactor  = altoPromedio * 0.5  → tolerancia de alineación dentro de una misma fila
    // ThresholdJumpFactor = altoPromedio * 1.5  → salto mínimo para considerar un cambio de bloque
    internal const double ThresholdRowFactor  = 0.5;
    internal const double ThresholdJumpFactor = 1.5;

    internal static void ReconstructPage(DocumentPage page, StringBuilder sb)
    {
        if (!(page.Lines?.Count > 0)) return;

        var pageWidth  = page.Width  is float pw and > 0 ? pw : 1f;
        var pageHeight = page.Height is float ph and > 0 ? ph : 1f;

        var projectedLines = ProjectLines(page, pageWidth, pageHeight);

        // Calcular el alto promedio de las líneas para derivar umbrales adaptativos
        var averageHeight = CalculateAverageHeight(page, pageHeight);
        var rowThreshold  = averageHeight * ThresholdRowFactor;
        var jumpThreshold = averageHeight * ThresholdJumpFactor;

        var rows = GroupIntoRows(projectedLines, rowThreshold);

        double? previousRowY = null;
        foreach (var row in rows)
        {
            // Línea en blanco entre bloques con salto vertical significativo
            if (previousRowY.HasValue && row[0].YRel - previousRowY.Value > jumpThreshold)
                sb.AppendLine();

            RenderRow(row, sb);
            previousRowY = row[0].YRel;
        }
    }

    /// <summary>
    /// Calcula el alto promedio de las líneas de la página en coordenadas relativas.
    /// Polygon es una lista plana [x0,y0, x1,y1, x2,y2, x3,y3]: índices 1 (y top) y 7 (y bottom-left).
    /// Si no hay líneas con suficientes vértices, devuelve un valor de reserva.
    /// </summary>
    internal static double CalculateAverageHeight(DocumentPage page, float pageHeight)
    {
        var linesWithHeight = (page.Lines ?? [])
            .Where(l => (l.Polygon?.Count ?? 0) >= 8)
            .Select(l => Math.Abs((l.Polygon![7] - l.Polygon![1]) / (double)pageHeight))
            .Where(h => h > 0)
            .ToList();

        // Valor de reserva: 1/50 del alto de página (~2%, equivalente a ~50 líneas por página)
        return linesWithHeight.Count > 0 ? linesWithHeight.Average() : 0.02;
    }

    /// <summary>Convierte cada línea del SDK a coordenadas relativas y las ordena arriba→abajo, izquierda→derecha.</summary>
    internal static List<ProjectedLine> ProjectLines(DocumentPage page, float pageWidth, float pageHeight) =>
        [.. (page.Lines ?? [])
            .Where(l => (l.Polygon?.Count ?? 0) >= 2)
            .Select(l => new ProjectedLine(
                l.Content,
                l.Polygon![0] / pageWidth,
                l.Polygon![1] / pageHeight))
            .OrderBy(l => l.YRel)
            .ThenBy(l => l.XRel)];

    /// <summary>Agrupa las líneas en filas visuales según su proximidad vertical.</summary>
    internal static List<List<ProjectedLine>> GroupIntoRows(List<ProjectedLine> lines, double rowThreshold)
    {
        List<List<ProjectedLine>> rows = [];
        foreach (var line in lines)
        {
            if (rows.Count == 0 || line.YRel - rows[^1][0].YRel > rowThreshold)
                rows.Add([]);
            rows[^1].Add(line);
        }
        return rows;
    }

    /// <summary>Escribe la fila: un fragmento directo, varios separados con " | " ordenados por X.</summary>
    internal static void RenderRow(List<ProjectedLine> row, StringBuilder sb)
    {
        if (row.Count == 1)
        {
            sb.AppendLine(row[0].Content.Trim());
            return;
        }

        sb.AppendLine(string.Join(Separator, row.OrderBy(l => l.XRel).Select(l => l.Content.Trim())));
    }
}

using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;

/// <summary>
/// Reconstruye el texto de una página agrupando líneas por posición vertical y horizontal.
/// </summary>
internal static class DocumentLayoutReconstructor
{
    /// <summary>Línea con coordenadas relativas (0..1) respecto al tamaño de la página.</summary>
    internal record struct LineaProyectada(string Contenido, double XRel, double YRel);

    /// <summary>Separador usado entre columnas de una misma fila visual.</summary>
    internal const string Separador = " | ";

    // Factores aplicados sobre el alto promedio de línea para calcular los umbrales dinámicos.
    // UmbralFila  = altoPromedio * 0.5  → tolerancia de alineación dentro de una misma fila
    // UmbralSalto = altoPromedio * 1.5  → salto mínimo para considerar un cambio de bloque
    internal const double FactorUmbralFila  = 0.5;
    internal const double FactorUmbralSalto = 1.5;

    internal static void ReconstruirPagina(DocumentPage page, StringBuilder sb)
    {
        if (page.Lines.Count == 0) return;

        var pageWidth  = page.Width.GetValueOrDefault(1f)  is float pw and > 0 ? pw : 1f;
        var pageHeight = page.Height.GetValueOrDefault(1f) is float ph and > 0 ? ph : 1f;

        var proyectadas = ProyectarLineas(page, pageWidth, pageHeight);

        // Calcular el alto promedio de las líneas para derivar umbrales adaptativos
        var altoPromedio = CalcularAltoPromedio(page, pageHeight);
        var umbralFila   = altoPromedio * FactorUmbralFila;
        var umbralSalto  = altoPromedio * FactorUmbralSalto;

        var filas = AgruparEnFilas(proyectadas, umbralFila);

        double? yFilaAnterior = null;
        foreach (var fila in filas)
        {
            // Línea en blanco entre bloques con salto vertical significativo
            if (yFilaAnterior.HasValue && fila[0].YRel - yFilaAnterior.Value > umbralSalto)
                sb.AppendLine();

            RenderizarFila(fila, sb);
            yFilaAnterior = fila[0].YRel;
        }
    }

    /// <summary>
    /// Calcula el alto promedio de las líneas de la página en coordenadas relativas,
    /// usando los vértices superior e inferior del BoundingPolygon (índices 0 y 3).
    /// Si no hay líneas con suficientes vértices, devuelve un valor de reserva.
    /// </summary>
    internal static double CalcularAltoPromedio(DocumentPage page, float pageHeight)
    {
        var lineasConAltura = page.Lines
            .Where(l => l.BoundingPolygon.Count >= 4)
            .Select(l => Math.Abs((double)(l.BoundingPolygon[3].Y - l.BoundingPolygon[0].Y) / pageHeight))
            .Where(h => h > 0)
            .ToList();

        // Valor de reserva: 1/50 del alto de página (~2%, equivalente a ~50 líneas por página)
        return lineasConAltura.Count > 0 ? lineasConAltura.Average() : 0.02;
    }

    /// <summary>Convierte cada línea del SDK a coordenadas relativas y las ordena arriba→abajo, izquierda→derecha.</summary>
    internal static List<LineaProyectada> ProyectarLineas(DocumentPage page, float pageWidth, float pageHeight) =>
        [.. page.Lines
            .Where(l => l.BoundingPolygon.Count >= 1)
            .Select(l => new LineaProyectada(
                l.Content,
                (double)l.BoundingPolygon[0].X / pageWidth,
                (double)l.BoundingPolygon[0].Y / pageHeight))
            .OrderBy(l => l.YRel)
            .ThenBy(l => l.XRel)];

    /// <summary>Agrupa las líneas en filas visuales según su proximidad vertical.</summary>
    internal static List<List<LineaProyectada>> AgruparEnFilas(List<LineaProyectada> lineas, double umbralFila)
    {
        var filas = new List<List<LineaProyectada>>();
        foreach (var linea in lineas)
        {
            if (filas.Count == 0 || linea.YRel - filas[^1][0].YRel > umbralFila)
                filas.Add([]);
            filas[^1].Add(linea);
        }
        return filas;
    }

    /// <summary>Escribe la fila: un fragmento directo, varios separados con " | " ordenados por X.</summary>
    internal static void RenderizarFila(List<LineaProyectada> fila, StringBuilder sb)
    {
        if (fila.Count == 1)
        {
            sb.AppendLine(fila[0].Contenido.Trim());
            return;
        }

        sb.AppendLine(string.Join(Separador, fila.OrderBy(l => l.XRel).Select(l => l.Contenido.Trim())));
    }
}

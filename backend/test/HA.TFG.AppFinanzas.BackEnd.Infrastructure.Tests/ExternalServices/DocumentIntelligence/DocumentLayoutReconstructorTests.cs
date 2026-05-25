using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence;
using System.Text;
using static HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.DocumentIntelligence.DocumentLayoutReconstructor;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.ExternalServices.DocumentIntelligence;

public class DocumentLayoutReconstructorTests
{
    // ─── AgruparEnFilas ───────────────────────────────────────────────────────

    [Fact]
    public void AgruparEnFilas_LineasConMismaY_SeAgrupaEnUnaFila()
    {
        var lineas = new List<LineaProyectada>
        {
            new("Concepto",  0.05, 0.10),
            new("Precio",    0.50, 0.10),
            new("Cantidad",  0.75, 0.10),
        };

        var filas = AgruparEnFilas(lineas, 0.012);

        Assert.Single(filas);
        Assert.Equal(3, filas[0].Count);
    }

    [Fact]
    public void AgruparEnFilas_LineasConDiferenteY_SeAgrupaEnFilasSeparadas()
    {
        var lineas = new List<LineaProyectada>
        {
            new("Encabezado", 0.05, 0.05),
            new("Detalle",    0.05, 0.30),
            new("Total",      0.05, 0.60),
        };

        var filas = AgruparEnFilas(lineas, 0.012);

        Assert.Equal(3, filas.Count);
    }

    [Fact]
    public void AgruparEnFilas_YDentroDelUmbral_SeAgrupaEnMismaFila()
    {
        // Diferencia de Y de 0.005, menor que el umbral de fila (~50% del alto de línea)
        var lineas = new List<LineaProyectada>
        {
            new("A", 0.05, 0.100),
            new("B", 0.50, 0.105),
        };

        var filas = AgruparEnFilas(lineas, 0.012);

        Assert.Single(filas);
    }

    [Fact]
    public void AgruparEnFilas_ListaVacia_DevuelveListaVacia()
    {
        var filas = AgruparEnFilas([], 0.012);

        Assert.Empty(filas);
    }

    // ─── RenderizarFila ───────────────────────────────────────────────────────

    [Fact]
    public void RenderizarFila_FilaSimple_DevuelveContenidoSinEspacios()
    {
        var fila = new List<LineaProyectada> { new("  Hiper Asia  ", 0.3, 0.1) };
        var sb = new StringBuilder();

        RenderizarFila(fila, sb);

        Assert.Equal("Hiper Asia", sb.ToString().Trim());
    }

    [Fact]
    public void RenderizarFila_FilaMultiColumna_SeparaConPipe()
    {
        var fila = new List<LineaProyectada>
        {
            new("Leche entera", 0.05, 0.10),
            new("1ud",          0.55, 0.10),
            new("0.95€",        0.75, 0.10),
        };
        var sb = new StringBuilder();

        RenderizarFila(fila, sb);

        var resultado = sb.ToString().Trim();
        Assert.Equal("Leche entera | 1ud | 0.95€", resultado);
    }

    [Fact]
    public void RenderizarFila_FilaMultiColumnaDesordenada_OrdenaDeLaIzquierdaADerecha()
    {
        // Los fragmentos llegan en orden inverso de X
        var fila = new List<LineaProyectada>
        {
            new("Importe", 0.80, 0.10),
            new("Cantidad", 0.55, 0.10),
            new("Concepto", 0.05, 0.10),
        };
        var sb = new StringBuilder();

        RenderizarFila(fila, sb);

        Assert.Equal("Concepto | Cantidad | Importe", sb.ToString().Trim());
    }

    // ─── ReconstruirPagina: salto de bloque ──────────────────────────────────

    [Fact]
    public void AgruparEnFilas_SaltoPorEncimaDelUmbral_InsertaFilaVaciaEntreBloques()
    {
        // Diferencia de Y de 0.05, mayor que umbralSalto (1.5x el alto de línea)
        var lineas = new List<LineaProyectada>
        {
            new("Encabezado", 0.05, 0.10),
            new("Total",      0.05, 0.15), // salto de 0.05 > umbralSalto
        };

        const double umbralFila  = 0.012;
        const double umbralSalto = 0.030;

        var sb = new StringBuilder();
        // Simular el loop de ReconstruirPagina manualmente
        var filas = AgruparEnFilas(lineas, umbralFila);
        double? yAnterior = null;
        foreach (var fila in filas)
        {
            if (yAnterior.HasValue && fila[0].YRel - yAnterior.Value > umbralSalto)
                sb.AppendLine();
            RenderizarFila(fila, sb);
            yAnterior = fila[0].YRel;
        }

        var lineasResultado = sb.ToString().Split(Environment.NewLine, StringSplitOptions.None);
        // Debe haber al menos una línea vacía entre los dos bloques
        Assert.Contains(lineasResultado, l => l == string.Empty);
    }
}

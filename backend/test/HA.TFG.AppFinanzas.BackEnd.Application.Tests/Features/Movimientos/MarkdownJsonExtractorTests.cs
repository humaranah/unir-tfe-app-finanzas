using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class MarkdownJsonExtractorTests
{
    [Fact]
    public void Extract_JsonPlano_DevuelveElMismoTexto()
    {
        var json = """{"establecimiento":"Tienda"}""";

        var result = MarkdownJsonExtractor.Extract(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void Extract_BloqueMarkdownJson_DevuelveJsonLimpio()
    {
        var respuesta = """
            ```json
            {"establecimiento":"Tienda","importe":3.90}
            ```
            """;

        var result = MarkdownJsonExtractor.Extract(respuesta);

        Assert.Equal("""{"establecimiento":"Tienda","importe":3.90}""", result);
    }

    [Fact]
    public void Extract_BloqueMarkdownSinEtiqueta_DevuelveJsonLimpio()
    {
        var respuesta = """
            ```
            {"establecimiento":"Tienda"}
            ```
            """;

        var result = MarkdownJsonExtractor.Extract(respuesta);

        Assert.Equal("""{"establecimiento":"Tienda"}""", result);
    }

    [Fact]
    public void Extract_TextoConEspaciosAlrededor_DevuelveTextoTrimado()
    {
        var respuesta = "   {\"establecimiento\":\"Tienda\"}   ";

        var result = MarkdownJsonExtractor.Extract(respuesta);

        Assert.Equal("""{"establecimiento":"Tienda"}""", result);
    }
}

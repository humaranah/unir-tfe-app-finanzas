using HA.TFG.AppFinanzas.BackEnd.Application;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;
using Microsoft.Extensions.Options;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class ProcesarComprobanteQueryValidatorTests
{
    private static readonly long MaxSizeBytes = 1 * 1024 * 1024;

    private static ProcesarComprobanteQueryValidator CrearValidator(long? maxSizeBytes = null) =>
        new(Options.Create(new ComprobanteConfig { MaxSizeBytes = maxSizeBytes ?? MaxSizeBytes }));

    // Magic bytes válidos
    private static readonly byte[] MagicJpeg = [0xFF, 0xD8, 0xFF, 0x00, 0x00];
    private static readonly byte[] MagicPdf  = [0x25, 0x50, 0x44, 0x46, 0x00]; // %PDF

    private static ProcesarComprobanteQuery CrearQuery(
        string contentType,
        byte[]? content = null,
        int? sizeOverride = null)
    {
        var bytes = content ?? MagicJpeg;
        Stream stream = sizeOverride.HasValue
            ? new FakeSizedStream(bytes, sizeOverride.Value)
            : new MemoryStream(bytes);

        return new ProcesarComprobanteQuery
        {
            ContentType       = contentType,
            ComprobanteStream = stream
        };
    }

    // ─── ContentType ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("application/pdf")]
    public void Validate_ContentTypePermitido_EsValido(string contentType)
    {
        var magic = contentType == "image/jpeg" ? MagicJpeg : MagicPdf;
        var query = CrearQuery(contentType, magic);

        var result = CrearValidator().Validate(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("application/octet-stream")]
    [InlineData("text/plain")]
    [InlineData("")]
    public void Validate_ContentTypeNoPermitido_TieneError(string contentType)
    {
        var query = CrearQuery(contentType);

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    // ─── Tamaño ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_StreamVacio_TieneError()
    {
        var query = new ProcesarComprobanteQuery
        {
            ContentType       = "image/jpeg",
            ComprobanteStream = new MemoryStream([])
        };

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    [Fact]
    public void Validate_StreamSuperaLimitePersonalizado_TieneError()
    {
        const long limitePersonalizado = 512;
        var query = CrearQuery("image/jpeg", sizeOverride: 513);

        var result = CrearValidator(limitePersonalizado).Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    [Fact]
    public void Validate_StreamSuperaUnMb_TieneError()
    {
        var query = CrearQuery("image/jpeg", sizeOverride: 1 * 1024 * 1024 + 1);

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    [Fact]
    public void Validate_StreamExactamenteUnMb_EsValido()
    {
        // 1 MB exacto con magic bytes JPEG al inicio
        var bytes = new byte[1 * 1024 * 1024];
        MagicJpeg.CopyTo(bytes, 0);
        var query = CrearQuery("image/jpeg", bytes);

        var result = CrearValidator().Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ContentTypeJpegConMagicBytesPdf_TieneError()
    {
        var query = CrearQuery("image/jpeg", MagicPdf);

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    [Fact]
    public void Validate_ContentTypePdfConMagicBytesJpeg_TieneError()
    {
        var query = CrearQuery("application/pdf", MagicJpeg);

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    [Fact]
    public void Validate_ContentTypePdfConMagicBytesPdf_EsValido()
    {
        var query = CrearQuery("application/pdf", MagicPdf);

        var result = CrearValidator().Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MagicBytesCorruptos_TieneError()
    {
        var query = CrearQuery("image/jpeg", [0x00, 0x00, 0x00]);

        var result = CrearValidator().Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "file");
    }

    // ─── Helper: stream con Length falso para simular superación de límite ────

    /// <summary>
    /// Stream cuyo <see cref="Length"/> reporta el valor indicado aunque el contenido
    /// real sean los bytes proporcionados. Permite testear la regla de tamaño máximo
    /// sin tener que alocar buffers de 1 MB+ en tests.
    /// </summary>
    private sealed class FakeSizedStream(byte[] content, long fakeLength) : MemoryStream(content)
    {
        public override long Length => fakeLength;
    }
}

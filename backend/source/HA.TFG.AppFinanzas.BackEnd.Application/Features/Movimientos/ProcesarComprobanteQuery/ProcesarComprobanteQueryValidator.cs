using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.ProcesarComprobanteQuery;

public sealed class ProcesarComprobanteQueryValidator : AbstractValidator<ProcesarComprobanteQuery>
{
    private const long MaxSizeBytes = 1 * 1024 * 1024; // 1 MB

    private static readonly Dictionary<string, byte[]> AllowedMagicBytes = new()
    {
        ["application/pdf"] = [0x25, 0x50, 0x44, 0x46],   // %PDF
        ["image/jpeg"]      = [0xFF, 0xD8, 0xFF]           // JFIF / EXIF
    };

    public ProcesarComprobanteQueryValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedMagicBytes.ContainsKey(ct?.ToLowerInvariant() ?? string.Empty))
            .WithName("file")
            .WithMessage("Tipo de archivo no admitido. Se aceptan únicamente PDF y JPEG.");

        RuleFor(x => x.ComprobanteStream)
            .NotNull()
            .WithName("file")
            .WithMessage("No se ha enviado ningún archivo.");

        RuleFor(x => x.ComprobanteStream)
            .Must(s => s is { Length: > 0 })
            .WithName("file")
            .WithMessage("El archivo está vacío.")
            .Must(s => s.Length <= MaxSizeBytes)
            .WithName("file")
            .WithMessage($"El archivo supera el tamaño máximo permitido de {MaxSizeBytes / 1024} KB.")
            .Must(HasValidMagicBytes)
            .WithName("file")
            .WithMessage("El contenido del archivo no coincide con el tipo declarado.")
            .When(x => x.ComprobanteStream is { Length: > 0 } &&
                        AllowedMagicBytes.ContainsKey(x.ContentType?.ToLowerInvariant() ?? string.Empty));
    }

    private bool HasValidMagicBytes(ProcesarComprobanteQuery query, Stream stream)
    {
        var expected = AllowedMagicBytes[query.ContentType.ToLowerInvariant()];
        var buffer = new byte[expected.Length];

        stream.Position = 0;
        var bytesRead = stream.Read(buffer, 0, expected.Length);
        stream.Position = 0;

        return bytesRead == expected.Length && buffer.AsSpan(0, expected.Length).SequenceEqual(expected);
    }
}

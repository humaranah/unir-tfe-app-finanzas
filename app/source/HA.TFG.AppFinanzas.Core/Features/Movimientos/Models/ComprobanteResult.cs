namespace HA.TFG.AppFinanzas.Core.Features.Movimientos.Models;

public sealed record ComprobanteResult(
    byte[] Bytes,
    string NombreArchivo,
    string ContentType);

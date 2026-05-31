namespace HA.TFG.AppFinanzas.Core.Movimientos;

public sealed record ComprobanteResult(
    byte[] Bytes,
    string NombreArchivo,
    string ContentType);

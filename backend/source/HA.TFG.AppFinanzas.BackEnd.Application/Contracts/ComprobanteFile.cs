namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public record ComprobanteFile(
    Stream Stream,
    string ContentType,
    string FileName);

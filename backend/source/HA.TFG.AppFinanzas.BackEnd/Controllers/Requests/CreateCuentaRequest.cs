namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record CreateCuentaRequest
{
    public required string Moneda { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}

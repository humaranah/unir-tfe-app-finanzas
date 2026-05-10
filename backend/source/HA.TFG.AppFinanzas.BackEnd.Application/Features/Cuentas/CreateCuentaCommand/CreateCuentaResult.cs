namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public record CreateCuentaResult
{
    public long IdCuenta { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public record CreateCuentaResult
{
    public Guid IdCuenta { get; init; }
    public string Moneda { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

public record GetCuentasResultItem
{
    public long Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}

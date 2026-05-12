namespace HA.TFG.AppFinanzas.Core.Cuentas;

public interface ICuentasService
{
    Task<bool> TieneCuentasAsync(CancellationToken cancellationToken = default);
    Task CrearCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default);
}

namespace HA.TFG.AppFinanzas.Core.Cuentas;

public interface ICuentasService
{
    Task<bool> TieneCuentasAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetDefaultCuentaIdAsync(CancellationToken cancellationToken = default);
    Task<string?> GetDefaultCuentaDescripcionAsync(CancellationToken cancellationToken = default);
    Task CreateCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default);
}

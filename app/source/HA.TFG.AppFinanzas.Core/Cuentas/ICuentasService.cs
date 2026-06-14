using HA.TFG.AppFinanzas.Core.Models.Enums;

namespace HA.TFG.AppFinanzas.Core.Cuentas;

public interface ICuentasService
{
    Task<bool> HasCuentasAsync(CancellationToken cancellationToken = default);
    Task<(Guid? Id, string? Descripcion)> GetDefaultCuentaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaItem>> GetCategoriasAsync(Guid idCuenta, CancellationToken cancellationToken = default);
    Task CreateCategoriaAsync(Guid idCuenta, string nombre, TipoMovimiento tipoMovimiento, CancellationToken cancellationToken = default);
    Task CreateCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default);
}

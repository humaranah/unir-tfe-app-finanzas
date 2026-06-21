using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class CuentaCategoriaRepository(AppDbContext context) : ICuentaCategoriaRepository
{
    public async Task<IReadOnlyList<CuentaCategoria>> GetCategoriasByCuentaAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken)
    {
        var existe = await context.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .SelectMany(u => u.Cuentas)
            .AnyAsync(c => c.IdCuenta == idCuenta, cancellationToken);

        if (!existe)
            throw new NotFoundException(nameof(Cuenta), idCuenta);

        return await context.CuentaCategorias
            .Where(cc => cc.IdCuenta == idCuenta && cc.FechaEliminacion == null)
            .OrderBy(cc => cc.TipoMovimiento)
            .ThenBy(cc => cc.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<CuentaCategoria?> GetCategoriaByNombreAsync(Guid idCuenta, string nombre, CancellationToken cancellationToken) =>
        context.CuentaCategorias
            .Where(cc => cc.IdCuenta == idCuenta && cc.Nombre == nombre && cc.FechaEliminacion == null)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<CuentaCategoria?> GetCategoriaByIdAsync(Guid idCuenta, Guid idCuentaCategoria, CancellationToken cancellationToken) =>
        context.CuentaCategorias
            .Where(cc => cc.IdCuenta == idCuenta && cc.IdCuentaCategoria == idCuentaCategoria && cc.FechaEliminacion == null)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CuentaCategoria> CreateCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken)
    {
        context.CuentaCategorias.Add(categoria);
        await context.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task<CuentaCategoria> UpdateCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken)
    {
        context.CuentaCategorias.Update(categoria);
        await context.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task DeleteCategoriaAsync(CuentaCategoria categoria, CancellationToken cancellationToken)
    {
        await context.CuentaCategorias
            .Where(cc => cc.IdCuentaCategoria == categoria.IdCuentaCategoria)
            .ExecuteUpdateAsync(s => s.SetProperty(cc => cc.FechaEliminacion, DateTime.UtcNow), cancellationToken);
    }
}

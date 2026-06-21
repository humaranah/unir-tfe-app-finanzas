using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public class CuentaRepository(AppDbContext context) : ICuentaRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(Guid idUsuario, CancellationToken cancellationToken)
    {
        var cuentas = await _context.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .SelectMany(u => u.Cuentas)
            .ToListAsync(cancellationToken);

        return cuentas ?? [];
    }

    public Task<Cuenta?> GetCuentaByIdAsync(Guid idUsuario, Guid idCuenta, CancellationToken cancellationToken) =>
        _context.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .SelectMany(u => u.Cuentas)
            .FirstOrDefaultAsync(c => c.IdCuenta == idCuenta, cancellationToken);

    public async Task<Cuenta> CreateCuentaWithCategoriasAsync(Cuenta cuenta, CancellationToken cancellationToken)
    {
        var categorias = await _context.Categorias.ToListAsync(cancellationToken);

        cuenta = cuenta with
        {
            FechaCreacion = DateTime.UtcNow,
            Categorias = [.. categorias.Select(c => new CuentaCategoria
            {
                IdCategoria = c.IdCategoria,
                TipoMovimiento = c.TipoMovimiento,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                FechaCreacion = DateTime.UtcNow
            })]
        };

        _context.Cuentas.Add(cuenta);
        await _context.SaveChangesAsync(cancellationToken);

        return cuenta;
    }
}

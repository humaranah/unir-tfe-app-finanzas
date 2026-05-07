using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public class CuentaRepository(AppDbContext context) : ICuentaRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(long idUsuario, CancellationToken cancellationToken)
    {
        var cuentas = await _context.Usuarios
            .Where(u => u.Id == idUsuario)
            .SelectMany(u => u.Cuentas)
            .ToListAsync(cancellationToken);

        return cuentas ?? [];
    }

    public Task<Cuenta?> GetCuentaByIdAsync(long idUsuario, long idCuenta, CancellationToken cancellationToken) =>
        _context.Usuarios
            .Where(u => u.Id == idUsuario)
            .SelectMany(u => u.Cuentas)
            .FirstOrDefaultAsync(c => c.Id == idCuenta, cancellationToken);

    public async Task<Cuenta> CreateCuentaConCategoriasAsync(long idUsuario, IReadOnlyList<Categoria> categorias, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios.FindAsync([idUsuario], cancellationToken)
            ?? throw new InvalidOperationException($"No se encontró el usuario con Id {idUsuario}.");

        var cuenta = new Cuenta
        {
            Nombre = "Mi cuenta",
            Descripcion = "Cuenta principal",
            FechaCreacion = DateTime.UtcNow,
            Usuarios = [usuario],
            Categorias = [.. categorias.Select(c => new CuentaCategoria
            {
                IdOrigen = c.Id,
                Slug = c.Slug,
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

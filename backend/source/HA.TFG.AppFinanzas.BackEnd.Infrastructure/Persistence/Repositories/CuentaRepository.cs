using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public class CuentaRepository(AppDbContext context) : ICuentaRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyList<Cuenta>> GetCuentasByUsuarioIdAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var cuentas = await _context.Usuarios
            .Where(u => u.Id == usuarioId)
            .SelectMany(u => u.Cuentas)
            .ToListAsync(cancellationToken);

        return cuentas ?? [];
    }

    public async Task<Cuenta> CreateCuentaConCategoriasAsync(long usuarioId, IReadOnlyList<Categoria> categorias, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios.FindAsync([usuarioId], cancellationToken)
            ?? throw new InvalidOperationException($"No se encontró el usuario con Id {usuarioId}.");

        var cuenta = new Cuenta
        {
            Nombre = "Mi cuenta",
            Descripcion = "Cuenta principal"
        };

        _context.Cuentas.Add(cuenta);
        await _context.SaveChangesAsync(cancellationToken);

        var usuarioCuenta = new UsuarioCuenta { IdUsuario = usuarioId, IdCuenta = cuenta.Id };
        _context.UsuariosCuentas.Add(usuarioCuenta);

        var cuentaCategorias = categorias.Select(c => new CuentaCategoria
        {
            IdCuenta = cuenta.Id,
            IdOrigen = c.Id,
            Slug = c.Slug,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            FechaCreacion = DateTime.UtcNow
        });

        _context.CuentaCategorias.AddRange(cuentaCategorias);
        await _context.SaveChangesAsync(cancellationToken);

        return cuenta;
    }
}

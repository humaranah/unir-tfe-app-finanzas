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
}

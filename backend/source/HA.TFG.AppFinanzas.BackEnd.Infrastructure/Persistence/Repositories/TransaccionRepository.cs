using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class TransaccionRepository(AppDbContext context) : ITransaccionRepository
{
    public async Task<IReadOnlyList<Transaccion>> GetTransaccionesAsync(
        long idCuenta,
        long? idCategoria,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        var query = context.Transacciones
            .Include(t => t.Categoria)
            .Where(t => t.IdCuenta == idCuenta);

        if (idCategoria.HasValue)
            query = query.Where(t => t.IdCategoria == idCategoria.Value);

        if (fechaDesde.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.Fecha) >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.Fecha) <= fechaHasta.Value);

        return await query
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }
}

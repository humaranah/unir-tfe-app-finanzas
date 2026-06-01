using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class MovimientoRepository(AppDbContext context) : IMovimientoRepository
{
    public async Task<IReadOnlyList<Movimiento>> GetMovimientosAsync(
        Guid idCuenta,
        Guid? idCategoria,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        var query = context.Movimientos
            .Include(t => t.Categoria)
            .Where(t => t.IdCuenta == idCuenta);

        if (idCategoria.HasValue)
            query = query.Where(t => t.IdCuentaCategoria == idCategoria.Value);

        if (fechaDesde.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.FechaMovimiento) >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.FechaMovimiento) <= fechaHasta.Value);

        return await query
            .OrderByDescending(t => t.FechaMovimiento)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movimiento> AddMovimientoAsync(Movimiento movimiento, CancellationToken cancellationToken)
    {
        context.Movimientos.Add(movimiento);
        await context.SaveChangesAsync(cancellationToken);
        return movimiento;
    }

    public Task<Movimiento?> GetMovimientoByIdAsync(Guid idCuenta, Guid idMovimiento, CancellationToken cancellationToken) =>
        context.Movimientos
            .Include(m => m.Categoria)
            .Where(m => m.IdCuenta == idCuenta && m.IdMovimiento == idMovimiento)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ResumenGastoCategoria>> GetResumenGastosPorCategoriaAsync(
        Guid accountId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        // El JOIN explícito con CuentaCategorias evita el acceso a la navegación dentro del GroupBy.
        // La proyección al tipo anónimo con Sum se resuelve en SQL; el mapeo al record se hace en memoria
        // para evitar que EF Core genere un AsQueryable().Sum() no traducible.
        var rows = await context.Movimientos
            .Where(m => m.IdCuenta == accountId
                && m.TipoMovimiento == TipoMovimiento.Gasto
                && DateOnly.FromDateTime(m.FechaMovimiento) >= from
                && DateOnly.FromDateTime(m.FechaMovimiento) <= to)
            .Join(
                context.CuentaCategorias,
                m => m.IdCuentaCategoria,
                c => c.IdCuentaCategoria,
                (m, c) => new
                {
                    m.FechaMovimiento.Year,
                    m.FechaMovimiento.Month,
                    m.IdCuentaCategoria,
                    CategoryName = c.Nombre,
                    Currency     = m.Moneda,
                    m.Importe
                })
            .GroupBy(x => new
            {
                x.Year,
                x.Month,
                x.IdCuentaCategoria,
                x.CategoryName,
                x.Currency
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.IdCuentaCategoria,
                g.Key.CategoryName,
                g.Key.Currency,
                Total = g.Sum(x => x.Importe)
            })
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ThenByDescending(r => r.Total)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new ResumenGastoCategoria(
            r.Year, r.Month, r.IdCuentaCategoria, r.CategoryName, r.Currency, r.Total))];
    }
}

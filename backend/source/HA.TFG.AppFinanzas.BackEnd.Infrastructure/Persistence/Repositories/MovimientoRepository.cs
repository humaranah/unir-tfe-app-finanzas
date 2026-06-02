using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories.Dtos;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories.Mappers;
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
        Guid idCuenta,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        CancellationToken cancellationToken)
    {
        // El JOIN se ejecuta en base de datos; el GroupBy+Sum se evalúa en cliente
        // para evitar que EF Core genere un AsQueryable().Sum() no traducible.
        var joinRows = await context.Movimientos
            .Where(m => m.IdCuenta == idCuenta
                && m.TipoMovimiento == TipoMovimiento.Gasto
                && DateOnly.FromDateTime(m.FechaMovimiento) >= fechaDesde
                && DateOnly.FromDateTime(m.FechaMovimiento) <= fechaHasta)
            .Join(
                context.CuentaCategorias,
                m => m.IdCuentaCategoria,
                c => c.IdCuentaCategoria,
                (m, c) => new
                {
                    Año = m.FechaMovimiento.Year,
                    Mes = m.FechaMovimiento.Month,
                    m.IdCuentaCategoria,
                    NombreCategoria = c.Nombre,
                    m.Moneda,
                    m.Importe
                })
            .ToListAsync(cancellationToken);

        return [.. joinRows
            .GroupBy(x => (x.Año, x.Mes, x.IdCuentaCategoria, x.NombreCategoria, x.Moneda))
            .Select(g => new ResumenGastoCategoriaRowDto(
                g.Key.Año,
                g.Key.Mes,
                g.Key.IdCuentaCategoria,
                g.Key.NombreCategoria,
                g.Key.Moneda,
                g.Sum(x => x.Importe)))
            .OrderByDescending(r => r.Año)
            .ThenByDescending(r => r.Mes)
            .ThenByDescending(r => r.Total)
            .Select(r => r.ToContract())];
    }
}

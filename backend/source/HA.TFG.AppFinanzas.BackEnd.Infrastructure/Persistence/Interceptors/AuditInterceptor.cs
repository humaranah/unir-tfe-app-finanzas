using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Interceptors;

internal class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void SetAuditFields(DbContext? context)
    {
        if (context == null)
            return;

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Movimiento>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(Movimiento.FechaCreacion)] = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.CurrentValues[nameof(Movimiento.FechaModificacion)] = now;
            }
        }
    }
}


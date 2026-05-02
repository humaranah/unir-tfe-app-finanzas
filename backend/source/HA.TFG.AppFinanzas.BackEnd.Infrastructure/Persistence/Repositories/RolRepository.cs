using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class RolRepository(AppDbContext context) : IRolRepository
{
    public Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken) =>
        context.Roles
            .FirstOrDefaultAsync(r => r.Nombre == nombre, cancellationToken);
}

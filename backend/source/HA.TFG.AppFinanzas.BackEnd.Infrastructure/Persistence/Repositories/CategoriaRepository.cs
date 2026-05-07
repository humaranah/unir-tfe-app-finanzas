using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public async Task<IReadOnlyList<Categoria>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Categorias.ToListAsync(cancellationToken);
}

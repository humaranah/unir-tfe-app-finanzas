using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> GetAllAsync(CancellationToken cancellationToken);
}

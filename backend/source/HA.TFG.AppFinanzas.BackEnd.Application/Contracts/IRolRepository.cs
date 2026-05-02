using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IRolRepository
{
    Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken);
}

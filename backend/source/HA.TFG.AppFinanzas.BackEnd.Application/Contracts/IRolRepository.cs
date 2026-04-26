using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Contracts;

public interface IRolRepository
{
    Task<Rol?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken);
}

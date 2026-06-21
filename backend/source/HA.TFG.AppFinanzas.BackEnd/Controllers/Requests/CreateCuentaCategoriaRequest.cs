using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record CreateCuentaCategoriaRequest
{
    public required string Nombre { get; init; }
    public TipoMovimiento TipoMovimiento { get; init; }
}

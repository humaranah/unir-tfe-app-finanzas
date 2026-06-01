using Mediator;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;

public record ObtenerRecomendacionesQuery : IRequest<RecomendacionResult>
{
    public required Guid IdCuenta { get; init; }
    public required string EmailUsuario { get; init; }

    /// <summary>Consulta adicional opcional del usuario dirigida al asistente financiero.</summary>
    public string? Consulta { get; init; }
}

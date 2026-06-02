namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record ObtenerRecomendacionesRequest
{
    public required Guid IdCuenta { get; init; }

    /// <summary>Consulta adicional opcional del usuario dirigida al asistente financiero.</summary>
    public string? Query { get; init; }
}

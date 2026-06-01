namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

public record ObtenerRecomendacionesRequest
{
    public Guid IdCuenta { get; init; }

    /// <summary>Consulta adicional opcional del usuario dirigida al asistente financiero.</summary>
    public string? Consulta { get; init; }
}

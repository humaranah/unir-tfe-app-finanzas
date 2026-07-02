namespace HA.TFG.AppFinanzas.Core.Features.Recomendaciones;

public interface IRecomendacionesService
{
    /// <summary>
    /// Obtiene recomendaciones financieras para la cuenta indicada. Si no se envía una
    /// consulta, devuelve un resumen basado en los gastos del mes y el historial.
    /// </summary>
    /// <param name="idCuenta">Identificador de la cuenta sobre la que generar recomendaciones.</param>
    /// <param name="query">Consulta opcional del usuario a modo de chat.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task<RecomendacionResult> GetRecomendacionAsync(
        Guid idCuenta,
        string? query = null,
        CancellationToken cancellationToken = default);
}

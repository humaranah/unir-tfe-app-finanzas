using HA.TFG.AppFinanzas.Core.Recomendaciones;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class RecomendacionesApiClient(IHttpClientFactory httpClientFactory) : IRecomendacionesService
{
    private record ObtenerRecomendacionesRequest(Guid IdCuenta, string? Query);

    public async Task<RecomendacionResult> GetRecomendacionAsync(
        Guid idCuenta,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.PostAsJsonAsync(
            "api/recomendaciones",
            new ObtenerRecomendacionesRequest(idCuenta, query),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al obtener recomendaciones. Status={(int)response.StatusCode}. Body={body}");
        }

        return await response.Content.ReadFromJsonAsync<RecomendacionResult>(cancellationToken)
            ?? throw new HttpRequestException("La respuesta de recomendaciones llegó vacía.");
    }
}

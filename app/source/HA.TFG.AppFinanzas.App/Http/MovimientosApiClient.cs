using HA.TFG.AppFinanzas.Core.Movimientos;
using System.Net.Http.Json;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed class MovimientosApiClient(IHttpClientFactory httpClientFactory) : IMovimientosService
{
    private record MovimientoResponse(
        Guid IdMovimiento,
        Guid IdCuenta,
        Guid? IdCategoria,
        string? NombreCategoria,
        string TipoMovimiento,
        string Concepto,
        decimal Importe,
        string Moneda,
        DateOnly FechaMovimiento);

    public async Task<IReadOnlyList<MovimientoItem>> GetMovimientosAsync(
        Guid idCuenta,
        GetMovimientosFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        var url = $"api/cuentas/{idCuenta}/movimientos{filters?.ToQueryString()}";

        using var response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al obtener movimientos. Status={(int)response.StatusCode}. Body={body}");
        }

        var items = await response.Content.ReadFromJsonAsync<List<MovimientoResponse>>(cancellationToken)
            ?? [];

        return items.Select(r => new MovimientoItem
        {
            IdMovimiento = r.IdMovimiento,
            IdCuenta = r.IdCuenta,
            IdCategoria = r.IdCategoria,
            NombreCategoria = r.NombreCategoria,
            TipoMovimiento = r.TipoMovimiento,
            Concepto = r.Concepto,
            Importe = r.Importe,
            Moneda = r.Moneda,
            FechaMovimiento = r.FechaMovimiento,
        }).ToList();
    }
}

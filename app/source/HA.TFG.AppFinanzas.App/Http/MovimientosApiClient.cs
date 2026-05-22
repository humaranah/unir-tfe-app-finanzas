using HA.TFG.AppFinanzas.App.Http.Mappers;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.App.Http;

internal sealed record MovimientoResponse(
    Guid IdMovimiento,
    Guid IdCuenta,
    Guid? IdCategoria,
    string? NombreCategoria,
    TipoMovimiento TipoMovimiento,
    string Concepto,
    decimal Importe,
    string Moneda,
    DateOnly FechaMovimiento);

internal sealed class MovimientosApiClient(IHttpClientFactory httpClientFactory) : IMovimientosService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

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

        var items = await response.Content.ReadFromJsonAsync<List<MovimientoResponse>>(JsonOptions, cancellationToken)
            ?? [];

        return [.. items.Select(MovimientoMapper.ToMovimientoItem)];
    }

    public async Task CreateMovimientoAsync(
        Guid idCuenta,
        string concepto,
        decimal importe,
        string moneda,
        TipoMovimiento tipo,
        DateOnly fecha,
        Guid? idCategoria = null,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var content = new MultipartFormDataContent
        {
            { new StringContent(concepto), "Concepto" },
            { new StringContent(importe.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Importe" },
            { new StringContent(moneda), "Moneda" },
            { new StringContent(tipo.ToString()), "TipoMovimiento" },
            { new StringContent(fecha.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-ddTHH:mm:ss")), "FechaMovimiento" }
        };
        if (idCategoria.HasValue)
            content.Add(new StringContent(idCategoria.Value.ToString()), "IdCuentaCategoria");

        using var response = await client.PostAsync(
            $"api/cuentas/{idCuenta}/movimientos", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al crear movimiento. Status={(int)response.StatusCode}. Body={responseBody}");
        }
    }
}

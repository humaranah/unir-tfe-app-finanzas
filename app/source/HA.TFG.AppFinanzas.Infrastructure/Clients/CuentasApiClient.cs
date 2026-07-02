using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HA.TFG.AppFinanzas.Infrastructure.Clients;

public sealed class CuentasApiClient(IHttpClientFactory httpClientFactory) : ICuentasService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private record CreateCuentaRequest(string Moneda, string Descripcion);
    private record CreateCategoriaRequest(string Nombre, TipoMovimiento TipoMovimiento);
    private record CuentaResponse(Guid Id, string Moneda, string Descripcion);
    private record CategoriaResponse(Guid IdCuentaCategoria, string Nombre, TipoMovimiento TipoMovimiento);

    public async Task<bool> HasCuentasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await GetCuentasAsync(cancellationToken);
        return cuentas.Count > 0;
    }

    public async Task<(Guid? Id, string? Descripcion)> GetDefaultCuentaAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await GetCuentasAsync(cancellationToken);
        return cuentas.Count > 0 ? (cuentas[0].Id, cuentas[0].Descripcion) : (null, null);
    }

    private async Task<List<CuentaResponse>> GetCuentasAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.GetAsync("api/cuentas", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al consultar cuentas. Status={(int)response.StatusCode}. Body={body}");
        }

        return await response.Content.ReadFromJsonAsync<List<CuentaResponse>>(cancellationToken) ?? [];
    }

    public async Task CreateCuentaAsync(string descripcion, string moneda, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.PostAsJsonAsync(
            "api/cuentas",
            new CreateCuentaRequest(moneda, descripcion),
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Error al crear cuenta. Status={(int)response.StatusCode}. Body={body}");
    }

    public async Task<IReadOnlyList<CategoriaItem>> GetCategoriasAsync(Guid idCuenta, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.GetAsync($"api/cuentas/{idCuenta}/categorias", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al obtener categorias. Status={(int)response.StatusCode}. Body={body}");
        }

        var items = await response.Content.ReadFromJsonAsync<List<CategoriaResponse>>(JsonOptions, cancellationToken) ?? [];

        return [.. items.Select(c => new CategoriaItem
        {
            IdCuentaCategoria = c.IdCuentaCategoria,
            Nombre = c.Nombre,
            TipoMovimiento = c.TipoMovimiento
        })];
    }

    public async Task CreateCategoriaAsync(Guid idCuenta, string nombre, TipoMovimiento tipoMovimiento, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.PostAsJsonAsync(
            $"api/cuentas/{idCuenta}/categorias",
            new CreateCategoriaRequest(nombre, tipoMovimiento),
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Error al crear categoría. Status={(int)response.StatusCode}. Body={body}");
    }

    public async Task UpdateCategoriaAsync(Guid idCuenta, Guid idCuentaCategoria, string nombre, TipoMovimiento tipoMovimiento, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.PutAsJsonAsync(
            $"api/cuentas/{idCuenta}/categorias/{idCuentaCategoria}",
            new CreateCategoriaRequest(nombre, tipoMovimiento),
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Error al actualizar categoría. Status={(int)response.StatusCode}. Body={body}");
    }

    public async Task DeleteCategoriaAsync(Guid idCuenta, Guid idCuentaCategoria, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(HttpClientNames.Backend);

        using var response = await client.DeleteAsync(
            $"api/cuentas/{idCuenta}/categorias/{idCuentaCategoria}",
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Error al eliminar categoría. Status={(int)response.StatusCode}. Body={body}");
    }
}

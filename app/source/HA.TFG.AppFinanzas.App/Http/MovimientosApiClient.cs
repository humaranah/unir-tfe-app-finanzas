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

internal sealed record MovimientoDetalleResponse(
    Guid IdMovimiento,
    Guid IdCuenta,
    Guid IdCuentaCategoria,
    string? NombreCategoria,
    TipoMovimiento TipoMovimiento,
    string Concepto,
    string? Establecimiento,
    decimal Importe,
    string Moneda,
    string? IdComprobante,
    string Nota,
    DateTime FechaMovimiento);

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
        CreateMovimientoDto dto,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var content = new MultipartFormDataContent
        {
            { new StringContent(dto.Concepto), "Concepto" },
            { new StringContent(dto.Importe.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Importe" },
            { new StringContent(dto.Moneda), "Moneda" },
            { new StringContent(dto.Tipo.ToString()), "TipoMovimiento" },
            { new StringContent(dto.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss")), "FechaMovimiento" },
            { new StringContent(dto.IdCuentaCategoria.ToString()), "IdCuentaCategoria" }
        };

        if (dto.Establecimiento is not null)
            content.Add(new StringContent(dto.Establecimiento), "Establecimiento");

        if (dto.Nota is not null)
            content.Add(new StringContent(dto.Nota), "Nota");

        if (dto.ComprobanteBytes is not null && dto.ComprobanteNombre is not null)
        {
            var fileContent = new ByteArrayContent(dto.ComprobanteBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(dto.ComprobanteContentType ?? "application/octet-stream");
            content.Add(fileContent, "Comprobante", dto.ComprobanteNombre);
        }

        using var response = await client.PostAsync(
            $"api/cuentas/{dto.IdCuenta}/movimientos", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al crear movimiento. Status={(int)response.StatusCode}. Body={responseBody}");
        }
    }

    public async Task UpdateMovimientoAsync(
        UpdateMovimientoDto dto,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var content = new MultipartFormDataContent
        {
            { new StringContent(dto.Concepto), "Concepto" },
            { new StringContent(dto.Importe.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Importe" },
            { new StringContent(dto.Moneda), "Moneda" },
            { new StringContent(dto.Tipo.ToString()), "TipoMovimiento" },
            { new StringContent(dto.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss")), "FechaMovimiento" },
            { new StringContent(dto.IdCuentaCategoria.ToString()), "IdCuentaCategoria" }
        };

        if (dto.Establecimiento is not null)
            content.Add(new StringContent(dto.Establecimiento), "Establecimiento");

        if (dto.Nota is not null)
            content.Add(new StringContent(dto.Nota), "Nota");

        if (dto.ComprobanteBytes is not null && dto.ComprobanteNombre is not null)
        {
            var fileContent = new ByteArrayContent(dto.ComprobanteBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(dto.ComprobanteContentType ?? "application/octet-stream");
            content.Add(fileContent, "Comprobante", dto.ComprobanteNombre);
        }

        using var response = await client.PutAsync(
            $"api/cuentas/{dto.IdCuenta}/movimientos/{dto.IdMovimiento}", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al actualizar movimiento. Status={(int)response.StatusCode}. Body={responseBody}");
        }
    }

    public async Task DeleteMovimientoAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.DeleteAsync(
            $"api/cuentas/{idCuenta}/movimientos/{idMovimiento}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al eliminar movimiento. Status={(int)response.StatusCode}. Body={responseBody}");
        }
    }

    public async Task<MovimientoDetalleItem> GetMovimientoDetalleAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.GetAsync(
            $"api/cuentas/{idCuenta}/movimientos/{idMovimiento}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al obtener detalle del movimiento. Status={(int)response.StatusCode}. Body={body}");
        }

        var detalle = await response.Content.ReadFromJsonAsync<MovimientoDetalleResponse>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("No se pudo deserializar el detalle del movimiento.");

        return new MovimientoDetalleItem
        {
            IdMovimiento = detalle.IdMovimiento,
            IdCuenta = detalle.IdCuenta,
            IdCuentaCategoria = detalle.IdCuentaCategoria,
            TipoMovimiento = detalle.TipoMovimiento,
            Concepto = detalle.Concepto,
            Establecimiento = detalle.Establecimiento,
            Importe = detalle.Importe,
            Moneda = detalle.Moneda,
            Nota = detalle.Nota,
            FechaMovimiento = detalle.FechaMovimiento,
            TieneComprobante = !string.IsNullOrEmpty(detalle.IdComprobante)
        };
    }

    public async Task<ComprobanteResult?> GetComprobanteAsync(
        Guid idCuenta,
        Guid idMovimiento,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var response = await client.GetAsync(
            $"api/cuentas/{idCuenta}/movimientos/{idMovimiento}/comprobante", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al obtener comprobante. Status={(int)response.StatusCode}. Body={body}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileName?.Trim('"')
            ?? contentDisposition?.FileNameStar?.Trim('"')
            ?? "comprobante";

        return new ComprobanteResult(bytes, fileName, contentType);
    }

    public async Task<ComprobanteExtraidoDto> EscanearComprobanteAsync(
        Guid idCuenta,
        ComprobanteResult comprobante,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Backend");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(comprobante.Bytes);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(comprobante.ContentType);
        content.Add(fileContent, "file", comprobante.NombreArchivo);
        content.Add(new StringContent(idCuenta.ToString()), "IdCuenta");

        using var response = await client.PostAsync("api/comprobantes/scan", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error al escanear comprobante. Status={(int)response.StatusCode}. Body={responseBody}");
        }

        return await response.Content.ReadFromJsonAsync<ComprobanteExtraidoDto>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("El servicio de escaneo no devolvió datos válidos.");
    }
}

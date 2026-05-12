using FluentValidation.Results;
using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Middleware;

public class ExceptionHandlerTests : IDisposable
{
    private IHost? _host;

    private HttpClient BuildClient(Exception exceptionToThrow)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseGlobalExceptionHandler();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/test", _ => throw exceptionToThrow);
                    });
                });
            });

        _host = builder.Start();
        return _host.GetTestClient();
    }

    [Fact]
    public async Task ValidationException_Devuelve400_ConErroresPorCampo()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "El email no puede estar vacío."),
            new("Moneda", "La moneda no puede estar vacía.")
        };
        using var client = BuildClient(new ValidationException(failures));

        using var response = await client.GetAsync("/test", CancellationToken.None);
        var rawJson = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"Email\"", rawJson);
        Assert.Contains("\"Moneda\"", rawJson);
        Assert.Contains("correlationId", rawJson);
    }

    [Fact]
    public async Task NotFoundException_Devuelve404_ConDetalle()
    {
        using var client = BuildClient(new NotFoundException("Usuario", "test@test.com"));

        using var response = await client.GetAsync("/test", CancellationToken.None);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Recurso no encontrado", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task ExternalServiceException_Devuelve502_ConDetalle()
    {
        using var client = BuildClient(new ExternalServiceException("Auth0", "No se pudo conectar con Auth0."));

        using var response = await client.GetAsync("/test", CancellationToken.None);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(502, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Error en servicio externo", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task ExcepcionNoControlada_Devuelve500()
    {
        using var client = BuildClient(new InvalidOperationException("Error inesperado."));

        using var response = await client.GetAsync("/test", CancellationToken.None);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Error interno del servidor", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    public void Dispose()
    {
        _host?.Dispose();
        GC.SuppressFinalize(this);
    }
}

using FluentValidation.Results;
using HA.TFG.AppFinanzas.BackEnd.Application.Common.Exceptions;
using HA.TFG.AppFinanzas.BackEnd.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Middleware;

public class ExceptionHandlerTests
{
    private static HttpClient BuildClient(Exception exceptionToThrow)
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

        var host = builder.Start();
        return host.GetTestClient();
    }

    [Fact]
    public async Task ValidationException_Devuelve400_ConErroresPorCampo()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "El email no puede estar vacío."),
            new("Moneda", "La moneda no puede estar vacía.")
        };
        var client = BuildClient(new ValidationException(failures));

        var response = await client.GetAsync("/test");
        var rawJson = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"Email\"", rawJson);
        Assert.Contains("\"Moneda\"", rawJson);
        Assert.Contains("correlationId", rawJson);
    }

    [Fact]
    public async Task NotFoundException_Devuelve404_ConDetalle()
    {
        var client = BuildClient(new NotFoundException("Usuario", "test@test.com"));

        var response = await client.GetAsync("/test");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Recurso no encontrado", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task ExternalServiceException_Devuelve502_ConDetalle()
    {
        var client = BuildClient(new ExternalServiceException("Auth0", "No se pudo conectar con Auth0."));

        var response = await client.GetAsync("/test");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(502, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Error en servicio externo", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task ExcepcionNoControlada_Devuelve500()
    {
        var client = BuildClient(new InvalidOperationException("Error inesperado."));

        var response = await client.GetAsync("/test");
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Error interno del servidor", json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }
}

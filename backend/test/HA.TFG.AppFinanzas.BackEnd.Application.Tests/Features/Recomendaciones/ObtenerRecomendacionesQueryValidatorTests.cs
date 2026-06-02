using HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Recomendaciones;

public class ObtenerRecomendacionesQueryValidatorTests
{
    private readonly ObtenerRecomendacionesQueryValidator _sut = new();

    [Fact]
    public void Validate_QueryValida_NoTieneErrores()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = "usuario@test.com"
        };

        var result = _sut.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_QueryValidaConConsulta_NoTieneErrores()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = "usuario@test.com",
            Query     = "¿Estoy gastando demasiado?"
        };

        var result = _sut.Validate(query);

        Assert.True(result.IsValid);
    }

    // ─── EmailUsuario ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmailVacio_TieneError(string email)
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = email
        };

        var result = _sut.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ObtenerRecomendacionesQuery.UserEmail));
    }

    // ─── IdCuenta ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_IdCuentaVacio_TieneError()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.Empty,
            UserEmail = "usuario@test.com"
        };

        var result = _sut.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ObtenerRecomendacionesQuery.IdCuenta));
    }

    // ─── Consulta ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ConsultaNula_NoTieneError()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = "usuario@test.com",
            Query     = null
        };

        var result = _sut.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ConsultaExcede500Caracteres_TieneError()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = "usuario@test.com",
            Query     = new string('a', 501)
        };

        var result = _sut.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ObtenerRecomendacionesQuery.Query));
    }

    [Fact]
    public void Validate_ConsultaExactamente500Caracteres_NoTieneError()
    {
        var query = new ObtenerRecomendacionesQuery
        {
            IdCuenta  = Guid.CreateVersion7(),
            UserEmail = "usuario@test.com",
            Query     = new string('a', 500)
        };

        var result = _sut.Validate(query);

        Assert.True(result.IsValid);
    }
}

using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class GetCuentasQueryValidatorTests
{
    private readonly GetCuentasQueryValidator _sut = new();

    [Fact]
    public void Validate_QueryValida_NoTieneErrores()
    {
        var query = new GetCuentasQuery("test@test.com");

        var result = _sut.Validate(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmailVacio_TieneError(string email)
    {
        var query = new GetCuentasQuery(email);

        var result = _sut.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetCuentasQuery.Email));
    }
}

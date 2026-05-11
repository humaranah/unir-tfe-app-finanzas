using HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Cuentas;

public class CreateCuentaCommandValidatorTests
{
    private readonly CreateCuentaCommandValidator _sut = new();

    [Fact]
    public void Validate_CommandValido_NoTieneErrores()
    {
        var command = new CreateCuentaCommand { Email = "test@test.com", Moneda = "EUR", Descripcion = "Mi cuenta" };

        var result = _sut.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmailVacio_TieneError(string email)
    {
        var command = new CreateCuentaCommand { Email = email, Moneda = "EUR" };

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_MonedaVacia_TieneError(string moneda)
    {
        var command = new CreateCuentaCommand { Email = "test@test.com", Moneda = moneda, Descripcion = "Mi cuenta" };

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.Moneda));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_DescripcionVacia_TieneError(string descripcion)
    {
        var command = new CreateCuentaCommand { Email = "test@test.com", Moneda = "EUR", Descripcion = descripcion };

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.Descripcion));
    }
}

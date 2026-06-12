using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class DeleteMovimientoCommandValidatorTests
{
    private readonly DeleteMovimientoCommandValidator _validator = new();

    private DeleteMovimientoCommand BuildCommand(
        string email = "test@test.com",
        Guid? idCuenta = null,
        Guid? idMovimiento = null) => new()
    {
        Email = email,
        IdCuenta = idCuenta ?? Guid.NewGuid(),
        IdMovimiento = idMovimiento ?? Guid.NewGuid()
    };

    [Fact]
    public void Validate_ComandoValido_SinErrores()
    {
        // Arrange
        var command = BuildCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmailInvalido_ConError(string email)
    {
        // Arrange
        var command = BuildCommand(email: email);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_IdCuentaVacio_ConError()
    {
        // Arrange
        var command = BuildCommand(idCuenta: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IdCuenta");
    }

    [Fact]
    public void Validate_IdMovimientoVacio_ConError()
    {
        // Arrange
        var command = BuildCommand(idMovimiento: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IdMovimiento");
    }
}

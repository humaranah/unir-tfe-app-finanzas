using FluentValidation.TestHelper;
using HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Movimientos;

public class UpdateMovimientoCommandValidatorTests
{
    private readonly UpdateMovimientoCommandValidator _validator = new();

    private UpdateMovimientoCommand BuildValidCommand() => new()
    {
        Email = "test@example.com",
        IdCuenta = Guid.NewGuid(),
        IdMovimiento = Guid.NewGuid(),
        IdCuentaCategoria = Guid.NewGuid(),
        TipoMovimiento = TipoMovimiento.Gasto,
        Concepto = "Compra",
        Importe = 50m,
        Moneda = "EUR",
        FechaMovimiento = DateTime.UtcNow
    };

    [Fact]
    public void Validate_ComandoValido_NoTieneErrores()
    {
        var command = BuildValidCommand();
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmailVacio_TieneError()
    {
        var command = BuildValidCommand() with { Email = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_IdCuentaVacio_TieneError()
    {
        var command = BuildValidCommand() with { IdCuenta = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.IdCuenta);
    }

    [Fact]
    public void Validate_IdMovimientoVacio_TieneError()
    {
        var command = BuildValidCommand() with { IdMovimiento = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.IdMovimiento);
    }

    [Fact]
    public void Validate_ConceptoVacio_TieneError()
    {
        var command = BuildValidCommand() with { Concepto = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Concepto);
    }

    [Fact]
    public void Validate_ConceptoExcedeLongitud_TieneError()
    {
        var command = BuildValidCommand() with { Concepto = new string('a', 501) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Concepto);
    }

    [Fact]
    public void Validate_ImporteInvalido_TieneError()
    {
        var command = BuildValidCommand() with { Importe = -50m };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Importe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("EURO")]
    public void Validate_MonedaInvalida_TieneError(string moneda)
    {
        var command = BuildValidCommand() with { Moneda = moneda };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Moneda);
    }

    [Fact]
    public void Validate_TipoCambioInvalido_TieneError()
    {
        var command = BuildValidCommand() with { TipoCambio = -1.5m };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TipoCambio);
    }
}

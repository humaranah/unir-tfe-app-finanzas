using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Usuarios;

public class EnsureUsuarioCommandValidatorTests
{
    private readonly EnsureUsuarioCommandValidator _sut = new();

    [Fact]
    public void Validate_CommandValido_NoTieneErrores()
    {
        var command = new EnsureUsuarioCommand("auth0|123", "test@test.com", "Test User", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_IdAuth0Vacio_TieneError(string idAuth0)
    {
        var command = new EnsureUsuarioCommand(idAuth0, "test@test.com", "Test", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.IdAuth0));
    }

    [Fact]
    public void Validate_IdAuth0DemasiadoLargo_TieneError()
    {
        var command = new EnsureUsuarioCommand(new string('a', 101), "test@test.com", "Test", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.IdAuth0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmailVacio_TieneError(string email)
    {
        var command = new EnsureUsuarioCommand("auth0|123", email, "Test", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.Email));
    }

    [Fact]
    public void Validate_EmailFormato_Invalido_TieneError()
    {
        var command = new EnsureUsuarioCommand("auth0|123", "no-es-un-email", "Test", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.Email));
    }

    [Fact]
    public void Validate_EmailDemasiadoLargo_TieneError()
    {
        var command = new EnsureUsuarioCommand("auth0|123", new string('a', 250) + "@b.com", "Test", null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NombreVacio_TieneError(string nombre)
    {
        var command = new EnsureUsuarioCommand("auth0|123", "test@test.com", nombre, null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.Nombre));
    }

    [Fact]
    public void Validate_NombreDemasiadoLargo_TieneError()
    {
        var command = new EnsureUsuarioCommand("auth0|123", "test@test.com", new string('a', 256), null, null, true, null);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.Nombre));
    }
}

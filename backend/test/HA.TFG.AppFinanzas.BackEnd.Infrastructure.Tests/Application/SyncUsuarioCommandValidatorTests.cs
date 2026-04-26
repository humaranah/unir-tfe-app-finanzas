using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Application;

public class SyncUsuarioCommandValidatorTests
{
    private readonly SyncUsuarioCommandValidator _sut = new();

    [Fact]
    public void Validate_CommandValido_NoTieneErrores()
    {
        var command = new SyncUsuarioCommand("auth0|123", "test@test.com", "Test User");

        var result = _sut.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_IdAuth0Vacio_TieneError(string idAuth0)
    {
        var command = new SyncUsuarioCommand(idAuth0, "test@test.com", "Test");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.IdAuth0));
    }

    [Fact]
    public void Validate_IdAuth0DemasiadoLargo_TieneError()
    {
        var command = new SyncUsuarioCommand(new string('a', 101), "test@test.com", "Test");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.IdAuth0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmailVacio_TieneError(string email)
    {
        var command = new SyncUsuarioCommand("auth0|123", email, "Test");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.Email));
    }

    [Fact]
    public void Validate_EmailFormato_Invalido_TieneError()
    {
        var command = new SyncUsuarioCommand("auth0|123", "no-es-un-email", "Test");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.Email));
    }

    [Fact]
    public void Validate_EmailDemasiadoLargo_TieneError()
    {
        var command = new SyncUsuarioCommand("auth0|123", new string('a', 250) + "@b.com", "Test");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NombreVacio_TieneError(string nombre)
    {
        var command = new SyncUsuarioCommand("auth0|123", "test@test.com", nombre);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.Nombre));
    }

    [Fact]
    public void Validate_NombreDemasiadoLargo_TieneError()
    {
        var command = new SyncUsuarioCommand("auth0|123", "test@test.com", new string('a', 256));

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SyncUsuarioCommand.Nombre));
    }
}

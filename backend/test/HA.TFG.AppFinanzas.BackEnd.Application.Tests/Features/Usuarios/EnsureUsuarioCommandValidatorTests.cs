using HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Tests.Features.Usuarios;

public class EnsureUsuarioCommandValidatorTests
{
    private readonly EnsureUsuarioCommandValidator _sut = new();

    [Fact]
    public void Validate_CommandValido_NoTieneErrores()
    {
        var command = new EnsureUsuarioCommand("auth0|123", "token_abc");

        var result = _sut.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_IdAuth0Vacio_TieneError(string idAuth0)
    {
        var command = new EnsureUsuarioCommand(idAuth0, "token_abc");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.IdAuth0));
    }

    [Fact]
    public void Validate_IdAuth0DemasiadoLargo_TieneError()
    {
        var command = new EnsureUsuarioCommand(new string('a', 101), "token_abc");

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.IdAuth0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_AccessTokenVacio_TieneError(string accessToken)
    {
        var command = new EnsureUsuarioCommand("auth0|123", accessToken);

        var result = _sut.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnsureUsuarioCommand.AccessToken));
    }
}

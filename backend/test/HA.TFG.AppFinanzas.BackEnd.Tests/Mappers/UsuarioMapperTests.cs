using HA.TFG.AppFinanzas.BackEnd.Mappers;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Mappers;

public class UsuarioMapperTests
{
    [Fact]
    public void ToSyncUsuarioCommand_MapeaTodasLasPropiedades()
    {
        // Arrange
        var claims = new Auth0UserClaims("auth0|123", "test@test.com", "Test User");

        // Act
        var result = UsuarioMapper.ToSyncUsuarioCommand(claims);

        // Assert
        Assert.Equal(claims.IdAuth0, result.IdAuth0);
        Assert.Equal(claims.Email, result.Email);
        Assert.Equal(claims.Nombre, result.Nombre);
    }

    [Fact]
    public void ToSyncUsuarioCommand_IdAuth0SeMapea()
    {
        var claims = new Auth0UserClaims("auth0|abc123", "a@b.com", "Nombre");

        var result = UsuarioMapper.ToSyncUsuarioCommand(claims);

        Assert.Equal("auth0|abc123", result.IdAuth0);
    }

    [Fact]
    public void ToSyncUsuarioCommand_EmailSeMapea()
    {
        var claims = new Auth0UserClaims("auth0|123", "correo@dominio.com", "Nombre");

        var result = UsuarioMapper.ToSyncUsuarioCommand(claims);

        Assert.Equal("correo@dominio.com", result.Email);
    }

    [Fact]
    public void ToSyncUsuarioCommand_NombreSeMapea()
    {
        var claims = new Auth0UserClaims("auth0|123", "a@b.com", "Juan Pérez");

        var result = UsuarioMapper.ToSyncUsuarioCommand(claims);

        Assert.Equal("Juan Pérez", result.Nombre);
    }
}

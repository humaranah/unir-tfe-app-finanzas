using HA.TFG.AppFinanzas.BackEnd.Mappers;
using System.Security.Claims;

namespace HA.TFG.AppFinanzas.BackEnd.Tests.Mappers;

public class ClaimsPrincipalMapperTests
{
    private static ClaimsPrincipal BuildUser(string? sub, string? email, string? nombre)
    {
        var claims = new List<Claim>();
        if (sub is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
        if (email is not null) claims.Add(new Claim("email", email));
        if (nombre is not null) claims.Add(new Claim("name", nombre));

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public void ToSyncUsuarioCommand_ClaimsCompletos_DevuelveCommand()
    {
        // Arrange
        var user = BuildUser("auth0|123", "test@test.com", "Test User");

        // Act
        var result = user.ToSyncUsuarioCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("auth0|123", result.IdAuth0);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Test User", result.Nombre);
    }

    [Fact]
    public void ToSyncUsuarioCommand_SinSub_DevuelveNull()
    {
        var user = BuildUser(sub: null, "test@test.com", "Test User");

        var result = user.ToSyncUsuarioCommand();

        Assert.Null(result);
    }

    [Fact]
    public void ToSyncUsuarioCommand_SinEmail_DevuelveNull()
    {
        var user = BuildUser("auth0|123", email: null, "Test User");

        var result = user.ToSyncUsuarioCommand();

        Assert.Null(result);
    }

    [Fact]
    public void ToSyncUsuarioCommand_SinNombre_DevuelveNull()
    {
        var user = BuildUser("auth0|123", "test@test.com", nombre: null);

        var result = user.ToSyncUsuarioCommand();

        Assert.Null(result);
    }

    [Fact]
    public void ToSyncUsuarioCommand_SubVacio_DevuelveNull()
    {
        var user = BuildUser("  ", "test@test.com", "Test User");

        var result = user.ToSyncUsuarioCommand();

        Assert.Null(result);
    }

    [Fact]
    public void ToSyncUsuarioCommand_UsaClaimSubComoFallback()
    {
        // Arrange — sin NameIdentifier, con claim "sub"
        var claims = new List<Claim>
        {
            new("sub", "auth0|fallback"),
            new("email", "test@test.com"),
            new("name", "Test User")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.ToSyncUsuarioCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("auth0|fallback", result.IdAuth0);
    }

    [Fact]
    public void ToSyncUsuarioCommand_UsaClaimEmailComoFallback()
    {
        // Arrange — sin claim "email", con ClaimTypes.Email
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "auth0|123"),
            new(ClaimTypes.Email, "fallback@test.com"),
            new("name", "Test User")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.ToSyncUsuarioCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fallback@test.com", result.Email);
    }
}

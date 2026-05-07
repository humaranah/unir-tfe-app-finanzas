using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence.Repositories;

public class UsuarioRepositoryTests : AppDbContextTestBase
{
    private readonly UsuarioRepository _sut;

    public UsuarioRepositoryTests()
    {
        _sut = new UsuarioRepository(Context);
    }

    [Fact]
    public async Task ObtenerPorIdAuth0Async_UsuarioExistente_DevuelveUsuario()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@test.com",
            Nombre = "Test",
            FechaCreacion = DateTime.UtcNow
        };
        var identidad = new UsuarioIdentidad { Id = 1, IdAuth0 = "auth0|123", Proveedor = "auth0", IdUsuario = 1 };
        Context.Usuarios.Add(usuario);
        Context.UsuarioIdentidades.Add(identidad);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetByIdAuth0Async("auth0|123", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuario.Email, result.Email);
    }

    [Fact]
    public async Task ObtenerPorIdAuth0Async_UsuarioNoExistente_DevuelveNull()
    {
        // Act
        var result = await _sut.GetByIdAuth0Async("auth0|noexiste", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPorIdAuth0Async_UsuarioEliminado_DevuelveNull()
    {
        // Arrange — soft delete activo
        var usuario = new Usuario
        {
            Id = 2,
            Email = "eliminado@test.com",
            Nombre = "Eliminado",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };
        var identidad = new UsuarioIdentidad { Id = 2, IdAuth0 = "auth0|eliminado", IdUsuario = 2 };
        Context.Usuarios.Add(usuario);
        Context.UsuarioIdentidades.Add(identidad);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetByIdAuth0Async("auth0|eliminado", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPorIdAuth0Async_IncludeRoles_DevuelveUsuarioConRoles()
    {
        // Arrange
        var rol = new Rol { Id = 1, Nombre = "usuario", FechaCreacion = DateTime.UtcNow };
        var usuario = new Usuario
        {
            Id = 3,
            Email = "conroles@test.com",
            Nombre = "Con Roles",
            FechaCreacion = DateTime.UtcNow,
            Roles = [rol]
        };
        var identidad = new UsuarioIdentidad { Id = 3, IdAuth0 = "auth0|conroles", IdUsuario = 3 };
        Context.Usuarios.Add(usuario);
        Context.UsuarioIdentidades.Add(identidad);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetByIdAuth0Async("auth0|conroles", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Roles);
        Assert.Equal("usuario", result.Roles.First().Nombre);
    }

    [Fact]
    public async Task CrearAsync_NuevoUsuario_LoGuardaEnBD()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 10,
            Email = "nuevo@test.com",
            Nombre = "Nuevo",
            FechaCreacion = DateTime.UtcNow
        };
        var identidad = new UsuarioIdentidad { IdAuth0 = "auth0|nuevo", Proveedor = "auth0" };

        // Act
        var result = await _sut.CreateAsync(usuario, identidad, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var enBD = await Context.Usuarios.FindAsync([result.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(enBD);
        Assert.Equal(usuario.Email, enBD.Email);
        var identidadEnBD = Context.UsuarioIdentidades.FirstOrDefault(i => i.IdUsuario == result.Id);
        Assert.NotNull(identidadEnBD);
        Assert.Equal("auth0|nuevo", identidadEnBD.IdAuth0);
    }

    [Fact]
    public async Task ActualizarAsync_UsuarioExistente_ActualizaDatos()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 20,
            Email = "viejo@test.com",
            Nombre = "Viejo",
            FechaCreacion = DateTime.UtcNow
        };
        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        var usuarioActualizado = usuario with
        {
            Email = "nuevo@test.com",
            Nombre = "Nuevo",
            FechaModificacion = DateTime.UtcNow
        };

        // Act
        var result = await _sut.UpdateAsync(usuarioActualizado, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("nuevo@test.com", result.Email);
        Assert.Equal("Nuevo", result.Nombre);
        var enBD = await Context.Usuarios.FindAsync([usuario.Id], TestContext.Current.CancellationToken);
        Assert.Equal("nuevo@test.com", enBD!.Email);
    }
}

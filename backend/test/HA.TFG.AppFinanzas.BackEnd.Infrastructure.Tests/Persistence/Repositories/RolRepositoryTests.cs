using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence.Repositories;

public class RolRepositoryTests : AppDbContextTestBase
{
    private readonly RolRepository _sut;

    public RolRepositoryTests()
    {
        _sut = new RolRepository(Context);
    }

    [Fact]
    public async Task ObtenerPorNombreAsync_RolExistente_DevuelveRol()
    {
        // Arrange
        var rol = new Rol { Id = 1, Nombre = "usuario", Descripcion = "Rol base", FechaCreacion = DateTime.UtcNow };
        Context.Roles.Add(rol);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetByNombreAsync("usuario", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("usuario", result.Nombre);
    }

    [Fact]
    public async Task ObtenerPorNombreAsync_RolNoExistente_DevuelveNull()
    {
        // Act
        var result = await _sut.GetByNombreAsync("noexiste", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPorNombreAsync_RolEliminado_DevuelveNull()
    {
        // Arrange — soft delete activo
        var rol = new Rol
        {
            Id = 2,
            Nombre = "eliminado",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };
        Context.Roles.Add(rol);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetByNombreAsync("eliminado", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPorNombreAsync_NombreDistinto_NoDevuelveRol()
    {
        // Arrange
        var rol = new Rol { Id = 3, Nombre = "usuario", FechaCreacion = DateTime.UtcNow };
        Context.Roles.Add(rol);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act — búsqueda con nombre diferente
        var result = await _sut.GetByNombreAsync("admin", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }
}

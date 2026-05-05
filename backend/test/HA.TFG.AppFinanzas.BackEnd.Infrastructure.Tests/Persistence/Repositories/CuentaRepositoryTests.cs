using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence.Repositories;

public class CuentaRepositoryTests : AppDbContextTestBase
{
    private readonly CuentaRepository _sut;

    public CuentaRepositoryTests()
    {
        _sut = new CuentaRepository(Context);
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_UsuarioConCuentas_DevuelveCuentas()
    {
        // Arrange
        var cuenta1 = new Cuenta { Id = 1, Nombre = "Cuenta 1", Descripcion = "Desc 1" };
        var cuenta2 = new Cuenta { Id = 2, Nombre = "Cuenta 2", Descripcion = "Desc 2" };
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@test.com",
            Nombre = "Test",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuenta1, cuenta2]
        };
        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(1, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Nombre == "Cuenta 1");
        Assert.Contains(result, c => c.Nombre == "Cuenta 2");
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_UsuarioSinCuentas_DevuelveListaVacia()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 2,
            Email = "sincuentas@test.com",
            Nombre = "Sin Cuentas",
            FechaCreacion = DateTime.UtcNow
        };
        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(2, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_UsuarioNoExistente_DevuelveListaVacia()
    {
        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(999, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_SoloDevuelveCuentasDelUsuarioSolicitado()
    {
        // Arrange
        var cuentaU1 = new Cuenta { Id = 3, Nombre = "Cuenta Usuario 1", Descripcion = "Desc" };
        var cuentaU2 = new Cuenta { Id = 4, Nombre = "Cuenta Usuario 2", Descripcion = "Desc" };
        var usuario1 = new Usuario
        {
            Id = 3,
            Email = "usuario1@test.com",
            Nombre = "Usuario 1",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuentaU1]
        };
        var usuario2 = new Usuario
        {
            Id = 4,
            Email = "usuario2@test.com",
            Nombre = "Usuario 2",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuentaU2]
        };
        Context.Usuarios.AddRange(usuario1, usuario2);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(3, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Cuenta Usuario 1", result[0].Nombre);
    }
}

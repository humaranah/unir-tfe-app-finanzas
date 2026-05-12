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
        var cuenta1 = new Cuenta { IdCuenta = Guid.NewGuid(), Moneda = "EUR", Descripcion = "Desc 1" };
        var cuenta2 = new Cuenta { IdCuenta = Guid.NewGuid(), Moneda = "EUR", Descripcion = "Desc 2" };
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "test@test.com",
            Nombre = "Test",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuenta1, cuenta2]
        };
        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(usuario.IdUsuario, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Descripcion == "Desc 1");
        Assert.Contains(result, c => c.Descripcion == "Desc 2");
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_UsuarioSinCuentas_DevuelveListaVacia()
    {
        // Arrange
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "sincuentas@test.com",
            Nombre = "Sin Cuentas",
            FechaCreacion = DateTime.UtcNow
        };
        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(usuario.IdUsuario, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_UsuarioNoExistente_DevuelveListaVacia()
    {
        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCuentasByUsuarioIdAsync_SoloDevuelveCuentasDelUsuarioSolicitado()
    {
        // Arrange
        var cuentaU1 = new Cuenta { IdCuenta = Guid.NewGuid(), Moneda = "EUR", Descripcion = "Desc Usuario 1" };
        var cuentaU2 = new Cuenta { IdCuenta = Guid.NewGuid(), Moneda = "EUR", Descripcion = "Desc Usuario 2" };
        var usuario1 = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "usuario1@test.com",
            Nombre = "Usuario 1",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuentaU1]
        };
        var usuario2 = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "usuario2@test.com",
            Nombre = "Usuario 2",
            FechaCreacion = DateTime.UtcNow,
            Cuentas = [cuentaU2]
        };
        Context.Usuarios.AddRange(usuario1, usuario2);
        await Context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _sut.GetCuentasByUsuarioIdAsync(usuario1.IdUsuario, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Desc Usuario 1", result[0].Descripcion);
    }
}

using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence;

public class SoftDeleteTests : AppDbContextTestBase
{
    [Fact]
    public async Task Usuario_eliminado_no_aparece_en_consulta()
    {
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "test@test.com",
            Nombre = "Test",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Context.Usuarios.ToListAsync(CancellationToken.None);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Usuario_activo_aparece_en_consulta()
    {
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "activo@test.com",
            Nombre = "Activo",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = null
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Context.Usuarios.ToListAsync(CancellationToken.None);

        Assert.Single(resultado);
    }

    [Fact]
    public async Task IgnoreQueryFilters_devuelve_registros_eliminados()
    {
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Email = "eliminado@test.com",
            Nombre = "Eliminado",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Context.Usuarios.IgnoreQueryFilters().ToListAsync(CancellationToken.None);

        Assert.Single(resultado);
    }

    [Fact]
    public async Task Cuenta_eliminada_no_aparece_en_consulta()
    {
        var cuenta = new Cuenta
        {
            IdCuenta = Guid.NewGuid(),
            Moneda = "EUR",
            Descripcion = "Cuenta test",
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Cuentas.Add(cuenta);
        await Context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Context.Cuentas.ToListAsync(CancellationToken.None);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Rol_eliminado_no_aparece_en_consulta()
    {
        var rol = new Rol
        {
            IdRol = Guid.NewGuid(),
            Nombre = "Admin",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Roles.Add(rol);
        await Context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Context.Roles.ToListAsync(CancellationToken.None);

        Assert.Empty(resultado);
    }
}

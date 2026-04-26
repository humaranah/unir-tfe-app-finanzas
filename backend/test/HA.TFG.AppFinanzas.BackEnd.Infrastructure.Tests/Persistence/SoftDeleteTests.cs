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
            Id = 1,
            IdAuth0 = 100,
            Email = "test@test.com",
            Nombre = "Test",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync();

        var resultado = await Context.Usuarios.ToListAsync();

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Usuario_activo_aparece_en_consulta()
    {
        var usuario = new Usuario
        {
            Id = 2,
            IdAuth0 = 200,
            Email = "activo@test.com",
            Nombre = "Activo",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = null
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync();

        var resultado = await Context.Usuarios.ToListAsync();

        Assert.Single(resultado);
    }

    [Fact]
    public async Task IgnoreQueryFilters_devuelve_registros_eliminados()
    {
        var usuario = new Usuario
        {
            Id = 3,
            IdAuth0 = 300,
            Email = "eliminado@test.com",
            Nombre = "Eliminado",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Usuarios.Add(usuario);
        await Context.SaveChangesAsync();

        var resultado = await Context.Usuarios.IgnoreQueryFilters().ToListAsync();

        Assert.Single(resultado);
    }

    [Fact]
    public async Task Cuenta_eliminada_no_aparece_en_consulta()
    {
        var cuenta = new Cuenta
        {
            Id = 1,
            Nombre = "Cuenta Test",
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Cuentas.Add(cuenta);
        await Context.SaveChangesAsync();

        var resultado = await Context.Cuentas.ToListAsync();

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Rol_eliminado_no_aparece_en_consulta()
    {
        var rol = new Rol
        {
            Id = 1,
            Nombre = "Admin",
            FechaCreacion = DateTime.UtcNow,
            FechaEliminacion = DateTime.UtcNow
        };

        Context.Roles.Add(rol);
        await Context.SaveChangesAsync();

        var resultado = await Context.Roles.ToListAsync();

        Assert.Empty(resultado);
    }
}

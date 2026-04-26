using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Domain;

public class SoftDeleteableTests
{
    [Fact]
    public void Usuario_implementa_ISoftDeleteable()
    {
        Assert.True(typeof(ISoftDeleteable).IsAssignableFrom(typeof(Usuario)));
    }

    [Fact]
    public void Rol_implementa_ISoftDeleteable()
    {
        Assert.True(typeof(ISoftDeleteable).IsAssignableFrom(typeof(Rol)));
    }

    [Fact]
    public void Cuenta_implementa_ISoftDeleteable()
    {
        Assert.True(typeof(ISoftDeleteable).IsAssignableFrom(typeof(Cuenta)));
    }

    [Fact]
    public void Usuario_FechaEliminacion_es_null_por_defecto()
    {
        var usuario = new Usuario();

        Assert.Null(usuario.FechaEliminacion);
    }

    [Fact]
    public void Rol_FechaEliminacion_es_null_por_defecto()
    {
        var rol = new Rol();

        Assert.Null(rol.FechaEliminacion);
    }

    [Fact]
    public void Cuenta_FechaEliminacion_es_null_por_defecto()
    {
        var cuenta = new Cuenta();

        Assert.Null(cuenta.FechaEliminacion);
    }
}

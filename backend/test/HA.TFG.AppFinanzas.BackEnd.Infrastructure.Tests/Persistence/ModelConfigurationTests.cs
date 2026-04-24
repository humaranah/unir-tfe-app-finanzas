using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Common;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests.Persistence;

public class ModelConfigurationTests : AppDbContextTestBase
{
    [Fact]
    public void AppDbContext_tiene_DbSet_para_todas_las_entidades()
    {
        Assert.NotNull(Context.Usuarios);
        Assert.NotNull(Context.Roles);
        Assert.NotNull(Context.Cuentas);
        Assert.NotNull(Context.Categorias);
        Assert.NotNull(Context.UsuarioCategorias);
        Assert.NotNull(Context.Transacciones);
        Assert.NotNull(Context.UsuariosRoles);
        Assert.NotNull(Context.UsuariosCuentas);
    }

    [Fact]
    public void Usuario_tiene_propiedad_FechaEliminacion()
    {
        var entityType = Context.Model.FindEntityType(typeof(Usuario));
        var property = entityType!.FindProperty(nameof(Usuario.FechaEliminacion));

        Assert.NotNull(property);
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void Rol_tiene_propiedad_FechaEliminacion()
    {
        var entityType = Context.Model.FindEntityType(typeof(Rol));
        var property = entityType!.FindProperty(nameof(Rol.FechaEliminacion));

        Assert.NotNull(property);
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void Cuenta_tiene_propiedad_FechaEliminacion()
    {
        var entityType = Context.Model.FindEntityType(typeof(Cuenta));
        var property = entityType!.FindProperty(nameof(Cuenta.FechaEliminacion));

        Assert.NotNull(property);
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void Usuario_tiene_filtro_global_soft_delete()
    {
        var entityType = Context.Model.FindEntityType(typeof(Usuario));

        Assert.NotNull(entityType!.GetQueryFilter());
    }

    [Fact]
    public void Rol_tiene_filtro_global_soft_delete()
    {
        var entityType = Context.Model.FindEntityType(typeof(Rol));

        Assert.NotNull(entityType!.GetQueryFilter());
    }

    [Fact]
    public void Cuenta_tiene_filtro_global_soft_delete()
    {
        var entityType = Context.Model.FindEntityType(typeof(Cuenta));

        Assert.NotNull(entityType!.GetQueryFilter());
    }
}

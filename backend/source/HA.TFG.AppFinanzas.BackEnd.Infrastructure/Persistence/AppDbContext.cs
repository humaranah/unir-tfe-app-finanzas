using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<UsuarioRol> UsuariosRoles { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<CuentaCategoria> CuentaCategorias { get; set; }
    public DbSet<Cuenta> Cuentas { get; set; }
    public DbSet<UsuarioCuenta> UsuariosCuentas { get; set; }
    public DbSet<Transaccion> Transacciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Filtro global de soft delete: excluir registros eliminados
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeleteable).IsAssignableFrom(entity.ClrType))
            {
                var parameter = Expression.Parameter(entity.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeleteable.FechaEliminacion));
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var filter = Expression.Lambda(Expression.Equal(property, nullConstant), parameter);

                modelBuilder.Entity(entity.ClrType).HasQueryFilter(filter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}

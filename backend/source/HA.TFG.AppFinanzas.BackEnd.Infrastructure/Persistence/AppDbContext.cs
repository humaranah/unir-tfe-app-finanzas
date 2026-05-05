using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<UsuarioIdentidad> UsuarioIdentidades { get; set; }
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

        SeedDatosIniciales(modelBuilder);
    }

    private static void SeedDatosIniciales(ModelBuilder modelBuilder)
    {
        var fecha = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "usuario", Descripcion = "Rol base asignado a todos los usuarios registrados", FechaCreacion = fecha }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Slug = "ingresos", Nombre = "Ingresos", Descripcion = "Cualquier tipo de ingreso", FechaCreacion = fecha },
            new Categoria { Id = 2, Slug = "gastos-vivienda", Nombre = "Vivienda", Descripcion = "Alquiler, hipoteca y gastos del hogar", FechaCreacion = fecha },
            new Categoria { Id = 3, Slug = "gastos-supermercado", Nombre = "Supermercado", Descripcion = "Compras en supermercado y alimentación en casa", FechaCreacion = fecha },
            new Categoria { Id = 4, Slug = "gastos-restaurantes", Nombre = "Restaurantes", Descripcion = "Restaurantes, bares y comida para llevar", FechaCreacion = fecha },
            new Categoria { Id = 5, Slug = "gastos-transporte", Nombre = "Transporte", Descripcion = "Combustible, transporte público y vehículo", FechaCreacion = fecha },
            new Categoria { Id = 6, Slug = "gastos-salud", Nombre = "Salud", Descripcion = "Médico, farmacia y seguros de salud", FechaCreacion = fecha },
            new Categoria { Id = 7, Slug = "gastos-educacion", Nombre = "Educación", Descripcion = "Cursos, libros y formación", FechaCreacion = fecha },
            new Categoria { Id = 8, Slug = "gastos-ocio", Nombre = "Ocio y entretenimiento", Descripcion = "Cine, viajes, hobbies y deportes", FechaCreacion = fecha },
            new Categoria { Id = 9, Slug = "gastos-ropa", Nombre = "Ropa y calzado", Descripcion = "Prendas de vestir y complementos", FechaCreacion = fecha },
            new Categoria { Id = 10, Slug = "gastos-tecnologia", Nombre = "Tecnología", Descripcion = "Dispositivos, software y suscripciones digitales", FechaCreacion = fecha },
            new Categoria { Id = 11, Slug = "gastos-servicios", Nombre = "Servicios", Descripcion = "Electricidad, agua, gas e internet", FechaCreacion = fecha },
            new Categoria { Id = 12, Slug = "gastos-otros", Nombre = "Otros gastos", Descripcion = "Gastos no clasificados", FechaCreacion = fecha }
        );
    }
}

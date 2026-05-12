using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Identidad> UsuarioIdentidades { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<UsuarioRol> UsuariosRoles { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<CuentaCategoria> CuentaCategorias { get; set; }
    public DbSet<Cuenta> Cuentas { get; set; }
    public DbSet<UsuarioCuenta> UsuariosCuentas { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }

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
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var categorias = new[]
        {
            (TipoMovimiento.Ingreso, "Ingresos",              "Cualquier tipo de ingreso"),
            (TipoMovimiento.Gasto,   "Vivienda",              "Alquiler, hipoteca y gastos del hogar"),
            (TipoMovimiento.Gasto,   "Supermercado",          "Compras en supermercado y alimentación en casa"),
            (TipoMovimiento.Gasto,   "Restaurantes",          "Restaurantes, bares y comida para llevar"),
            (TipoMovimiento.Gasto,   "Transporte",            "Combustible, transporte público y vehículo"),
            (TipoMovimiento.Gasto,   "Salud",                 "Médico, farmacia y seguros de salud"),
            (TipoMovimiento.Gasto,   "Educación",             "Cursos, libros y formación"),
            (TipoMovimiento.Gasto,   "Ocio y entretenimiento", "Cine, viajes, hobbies y deportes"),
            (TipoMovimiento.Gasto,   "Ropa y calzado",        "Prendas de vestir y complementos"),
            (TipoMovimiento.Gasto,   "Tecnología",            "Dispositivos, software y suscripciones digitales"),
            (TipoMovimiento.Gasto,   "Servicios",             "Electricidad, agua, gas e internet"),
            (TipoMovimiento.Gasto,   "Otros gastos",          "Gastos no clasificados"),
        };

        modelBuilder.Entity<Categoria>().HasData(
            categorias.Select(c => new Categoria
            {
                IdCategoria = GuidFromNombre(c.Item2),
                TipoMovimiento = c.Item1,
                Nombre = c.Item2,
                Descripcion = c.Item3,
                FechaCreacion = seedDate
            })
        );
    }

    /// <summary>
    /// Genera un GUID determinista a partir de un nombre usando SHA-256,
    /// de modo que el mismo nombre produce siempre el mismo GUID.
    /// </summary>
    private static Guid GuidFromNombre(string nombre)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(nombre));
        // Usar los primeros 16 bytes del hash como base del GUID
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(bytes);
        return new Guid(bytes);
    }
}

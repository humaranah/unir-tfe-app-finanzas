using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CuentaCategoriaConfigurations : IEntityTypeConfiguration<CuentaCategoria>
{
    public void Configure(EntityTypeBuilder<CuentaCategoria> builder)
    {
        builder.ToTable("cuenta_categorias");
        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.IdCuenta).IsRequired();
        builder.Property(categoria => categoria.Slug).IsRequired().HasMaxLength(100);
        builder.Property(categoria => categoria.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(categoria => categoria.Descripcion).HasMaxLength(500);
        builder.Property(categoria => categoria.FechaCreacion).IsRequired();
        // Relación: CuentaCategoria -> Cuenta (muchos a uno)
        builder
            .HasOne(categoria => categoria.Cuenta)
            .WithMany(cuenta => cuenta.Categorias)
            .HasForeignKey(categoria => categoria.IdCuenta)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación: CuentaCategoria -> Categoria (muchos a uno, opcional)
        builder
            .HasOne(categoria => categoria.Origen)
            .WithMany()
            .HasForeignKey(categoria => categoria.IdOrigen)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

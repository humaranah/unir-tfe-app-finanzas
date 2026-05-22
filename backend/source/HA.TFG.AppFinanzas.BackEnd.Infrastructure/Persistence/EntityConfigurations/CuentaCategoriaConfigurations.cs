using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CuentaCategoriaConfigurations : IEntityTypeConfiguration<CuentaCategoria>
{
    public void Configure(EntityTypeBuilder<CuentaCategoria> builder)
    {
        builder.ToTable("cuenta_categorias");
        builder.HasKey(cc => cc.IdCuentaCategoria);

        builder.Property(cc => cc.IdCuentaCategoria).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(cc => cc.IdCuenta).IsRequired();
        builder.Property(cc => cc.TipoMovimiento)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TipoMovimiento>())
            .HasMaxLength(20);
        builder.Property(cc => cc.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(cc => cc.Descripcion).HasMaxLength(500);
        builder.Property(cc => cc.FechaCreacion).IsRequired();
        builder.Property(cc => cc.FechaModificacion).IsRequired(false);
        builder.Property(cc => cc.FechaEliminacion).IsRequired(false);

        builder.HasIndex(cc => new { cc.IdCuenta, cc.Nombre }).IsUnique();
        builder.HasIndex(cc => new { cc.IdCuentaCategoria, cc.IdCuenta }).IsUnique();

        // Relación: CuentaCategoria -> Cuenta (muchos a uno)
        builder
            .HasOne(cc => cc.Cuenta)
            .WithMany(cuenta => cuenta.Categorias)
            .HasForeignKey(cc => cc.IdCuenta)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación: CuentaCategoria -> Categoria (muchos a uno, opcional)
        builder
            .HasOne(cc => cc.Origen)
            .WithMany()
            .HasForeignKey(cc => cc.IdCategoria)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

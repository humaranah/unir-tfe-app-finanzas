using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class MovimientoConfigurations : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("movimientos");
        builder.HasKey(movimiento => movimiento.IdMovimiento);

        builder.Property(movimiento => movimiento.IdMovimiento).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(movimiento => movimiento.IdCuenta).IsRequired();
        builder.Property(movimiento => movimiento.IdCategoria).IsRequired();
        builder.Property(movimiento => movimiento.TipoMovimiento)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TipoMovimiento>())
            .HasMaxLength(20);
        builder.Property(movimiento => movimiento.Concepto).HasMaxLength(500);
        builder.Property(movimiento => movimiento.Importe).HasPrecision(18, 2).IsRequired();
        builder.Property(movimiento => movimiento.Moneda).IsRequired().HasMaxLength(3);
        builder.Property(movimiento => movimiento.TipoCambio).HasPrecision(18, 6);
        builder.Property(movimiento => movimiento.IdComprobante).HasMaxLength(100);
        builder.Property(movimiento => movimiento.Nota).HasMaxLength(1000);
        builder.Property(movimiento => movimiento.FechaMovimiento).IsRequired();
        builder.Property(movimiento => movimiento.FechaCreacion).IsRequired();
        builder.Property(movimiento => movimiento.FechaModificacion).IsRequired(false);
        builder.Property(movimiento => movimiento.FechaEliminacion).IsRequired(false);

        // Relación: Movimiento -> Cuenta (muchos a uno)
        // Restrict para evitar rutas de cascada múltiple en SQL Server
        builder
            .HasOne(movimiento => movimiento.Cuenta)
            .WithMany()
            .HasForeignKey(movimiento => movimiento.IdCuenta)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación: Movimiento -> CuentaCategoria (muchos a uno)
        builder
            .HasOne(movimiento => movimiento.Categoria)
            .WithMany(categoria => categoria.Movimientos)
            .HasForeignKey(movimiento => movimiento.IdCategoria)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

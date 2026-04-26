using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class TransaccionConfigurations : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("transacciones");
        builder.HasKey(transaccion => transaccion.Id);

        builder.Property(transaccion => transaccion.IdCuenta).IsRequired();
        builder.Property(transaccion => transaccion.IdCategoria).IsRequired();
        builder.Property(transaccion => transaccion.Descripcion).HasMaxLength(500);
        builder.Property(transaccion => transaccion.Monto).HasPrecision(18, 2).IsRequired();
        builder.Property(transaccion => transaccion.Moneda).IsRequired().HasMaxLength(3);
        builder.Property(transaccion => transaccion.MonedaLocal).IsRequired().HasMaxLength(3);
        builder.Property(transaccion => transaccion.TipoCambio).HasPrecision(18, 6).IsRequired();
        builder.Property(transaccion => transaccion.Notas).HasMaxLength(1000);
        builder.Property(transaccion => transaccion.Fecha).IsRequired();
        builder.Property(transaccion => transaccion.FechaCreacion).IsRequired();

        // Relación: Transaccion -> Cuenta (muchos a uno)
        // NoAction para evitar rutas de cascada múltiple en SQL Server
        builder
            .HasOne(transaccion => transaccion.Cuenta)
            .WithMany()
            .HasForeignKey(transaccion => transaccion.IdCuenta)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación: Transaccion -> CuentaCategoria (muchos a uno)
        builder
            .HasOne(transaccion => transaccion.Categoria)
            .WithMany(categoria => categoria.Transacciones)
            .HasForeignKey(transaccion => transaccion.IdCategoria)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

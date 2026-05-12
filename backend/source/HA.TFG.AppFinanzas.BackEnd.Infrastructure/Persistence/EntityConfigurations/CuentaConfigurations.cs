using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CuentaConfigurations : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuentas");
        builder.HasKey(cuenta => cuenta.IdCuenta);

        builder.Property(cuenta => cuenta.IdCuenta).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(cuenta => cuenta.Moneda).IsRequired().HasMaxLength(10);
        builder.Property(cuenta => cuenta.Descripcion).HasMaxLength(500);
        builder.Property(cuenta => cuenta.FechaCreacion).IsRequired();
        builder.Property(cuenta => cuenta.FechaModificacion).IsRequired(false);
        builder.Property(cuenta => cuenta.FechaEliminacion).IsRequired(false);
    }
}

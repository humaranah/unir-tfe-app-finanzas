using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CuentaConfigurations : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuentas");
        builder.HasKey(cuenta => cuenta.Id);

        builder.Property(cuenta => cuenta.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(cuenta => cuenta.Descripcion).HasMaxLength(500);
        builder.Property(cuenta => cuenta.FechaEliminacion).IsRequired(false);
    }
}

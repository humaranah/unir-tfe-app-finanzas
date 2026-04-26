using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class RolConfigurations : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(rol => rol.Id);

        builder.Property(rol => rol.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(rol => rol.Descripcion).HasMaxLength(500);
        builder.Property(rol => rol.FechaCreacion).IsRequired();
        builder.Property(rol => rol.FechaEliminacion).IsRequired(false);
    }
}

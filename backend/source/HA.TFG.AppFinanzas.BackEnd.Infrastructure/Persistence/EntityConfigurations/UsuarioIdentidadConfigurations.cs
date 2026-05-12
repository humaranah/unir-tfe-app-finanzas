using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class UsuarioIdentidadConfigurations : IEntityTypeConfiguration<Identidad>
{
    public void Configure(EntityTypeBuilder<Identidad> builder)
    {
        builder.ToTable("usuario_identidades");
        builder.HasKey(x => x.IdIdentidad);

        builder.Property(x => x.IdIdentidad).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(x => x.IdAuth0).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.IdAuth0).IsUnique();
        builder.Property(x => x.Proveedor).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.IdUsuario).IsRequired();
    }
}

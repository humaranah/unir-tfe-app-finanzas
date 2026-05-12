using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CategoriaConfigurations : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(categoria => categoria.IdCategoria);

        builder.Property(categoria => categoria.IdCategoria).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(categoria => categoria.TipoMovimiento)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TipoMovimiento>())
            .HasMaxLength(20);
        builder.Property(categoria => categoria.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(categoria => categoria.Descripcion).HasMaxLength(500);
        builder.Property(categoria => categoria.FechaCreacion).IsRequired();

        builder.HasIndex(categoria => categoria.Nombre).IsUnique();
    }
}

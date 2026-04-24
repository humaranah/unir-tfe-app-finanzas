using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class CategoriaConfigurations : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(categoria => categoria.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(categoria => categoria.Descripcion)
            .HasMaxLength(500);

        builder.Property(categoria => categoria.FechaCreacion)
            .IsRequired();
    }
}

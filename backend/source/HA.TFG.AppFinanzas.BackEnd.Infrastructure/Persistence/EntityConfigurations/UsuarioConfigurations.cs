using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class UsuarioConfigurations : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.IdUsuario);

        builder.Property(x => x.IdUsuario).HasValueGenerator<GuidV7ValueGenerator>();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FotoPerfil).HasMaxLength(2048).IsRequired(false);
        builder.Property(x => x.EmailVerificado).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.FechaModificacion).IsRequired(false);
        builder.Property(x => x.FechaEliminacion).IsRequired(false);

        // Relación one-to-many: Usuario -> Identidad
        builder
            .HasMany(x => x.Identidades)
            .WithOne()
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación many-to-many: Usuario -> Rol
        builder
            .HasMany(x => x.Roles)
            .WithMany(rol => rol.Usuarios)
            .UsingEntity<UsuarioRol>(
                left => left.HasOne<Rol>().WithMany().HasForeignKey(x => x.IdRol),
                right => right.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario))
            .ToTable("usuario_roles")
            .HasKey(x => new { x.IdUsuario, x.IdRol });

        // Relación many-to-many: Usuario -> Cuenta
        builder
            .HasMany(x => x.Cuentas)
            .WithMany(cuenta => cuenta.Usuarios)
            .UsingEntity<UsuarioCuenta>(
                left => left.HasOne<Cuenta>().WithMany().HasForeignKey(x => x.IdCuenta),
                right => right.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario))
            .ToTable("usuario_cuentas")
            .HasKey(x => new { x.IdUsuario, x.IdCuenta });
    }
}

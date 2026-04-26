using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.EntityConfigurations;

internal class UsuarioConfigurations : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdAuth0).IsRequired();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Metadata).HasMaxLength(2000);
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.FechaEliminacion).IsRequired(false);

        // Relación many-to-many: Usuario -> Rol (skip navigation a través de UsuarioRol)
        builder
            .HasMany(x => x.Roles)
            .WithMany(rol => rol.Usuarios)
            .UsingEntity<UsuarioRol>(
                left => left.HasOne<Rol>().WithMany().HasForeignKey(x => x.IdRol),
                right => right.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario))
            .ToTable("usuario_roles")
            .HasKey(x => new { x.IdUsuario, x.IdRol });

        // Relación many-to-many: Usuario -> Cuenta (skip navigation a través de UsuarioCuenta)
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

using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAuth0Async(string idAuth0, CancellationToken cancellationToken) =>
        context.Usuarios
            .Include(u => u.Identidades)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(
                u => u.Identidades.Any(i => i.IdAuth0 == idAuth0),
                cancellationToken);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Usuarios
            .Include(u => u.Identidades)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario> CreateAsync(Usuario usuario, Identidad identidad, CancellationToken cancellationToken)
    {
        var usuarioConIdentidad = usuario with { Identidades = [identidad] };
        context.Usuarios.Add(usuarioConIdentidad);
        await context.SaveChangesAsync(cancellationToken);
        return usuarioConIdentidad;
    }

    public async Task AddIdentidadAsync(Guid idUsuario, Identidad identidad, CancellationToken cancellationToken)
    {
        var identidadConId = identidad with { IdUsuario = idUsuario };
        context.UsuarioIdentidades.Add(identidadConId);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var entry = context.Entry(usuario);
        if (entry.State == EntityState.Detached)
        {
            var tracked = await context.Usuarios.FindAsync([usuario.IdUsuario], cancellationToken);
            if (tracked is not null)
                context.Entry(tracked).CurrentValues.SetValues(usuario);
        }
        await context.SaveChangesAsync(cancellationToken);
        return usuario;
    }
}

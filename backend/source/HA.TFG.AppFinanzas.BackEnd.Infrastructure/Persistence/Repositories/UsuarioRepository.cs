using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using HA.TFG.AppFinanzas.BackEnd.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HA.TFG.AppFinanzas.BackEnd.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAuth0Async(string idAuth0, CancellationToken cancellationToken) =>
        context.Usuarios
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.IdAuth0 == idAuth0, cancellationToken);

    public async Task<Usuario> CrearAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var entry = context.Entry(usuario);
        if (entry.State == EntityState.Detached)
        {
            var tracked = await context.Usuarios.FindAsync([usuario.Id], cancellationToken);
            if (tracked is not null)
                context.Entry(tracked).CurrentValues.SetValues(usuario);
        }
        await context.SaveChangesAsync(cancellationToken);
        return usuario;
    }
}

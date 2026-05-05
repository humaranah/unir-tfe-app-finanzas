using HA.TFG.AppFinanzas.BackEnd.Application.Contracts;
using System.Security.Claims;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.BackEnd.Middleware;

public sealed class ResolveUsuarioMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUsuarioRepository usuarioRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idAuth0 = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(idAuth0))
            {
                var usuario = await usuarioRepository.GetByIdAuth0Async(idAuth0, context.RequestAborted);

                if (usuario is not null)
                {
                    var identity = (ClaimsIdentity)context.User.Identity;
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Email));
                }
            }
        }

        await next(context);
    }
}

using HA.TFG.AppFinanzas.BackEnd.Auth;
using HA.TFG.AppFinanzas.BackEnd.Domain.Common;
using HA.TFG.AppFinanzas.BackEnd.Infrastructure.ExternalServices.Auth0;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace HA.TFG.AppFinanzas.BackEnd.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth0(this IServiceCollection services, IHostEnvironment env)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events.OnAuthenticationFailed = ctx =>
                {
                    if (env.IsDevelopment())
                        Log.Debug(ctx.Exception, "[JWT] Autenticación fallida");
                    return Task.CompletedTask;
                };
            });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<Auth0Config>>((jwtOptions, auth0) =>
            {
                jwtOptions.Authority = auth0.Value.Domain;
                jwtOptions.Audience = auth0.Value.Audience;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Usuario, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole(Roles.Usuario));
        services.AddScoped<IClaimsTransformation, RolesClaimsTransformation>();

        return services;
    }
}

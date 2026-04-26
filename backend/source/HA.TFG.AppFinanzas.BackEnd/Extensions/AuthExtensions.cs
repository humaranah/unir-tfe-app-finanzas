using HA.TFG.AppFinanzas.BackEnd.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HA.TFG.AppFinanzas.BackEnd.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth0(this IServiceCollection services, IConfiguration configuration)
    {
        var domain = configuration["Auth0:Domain"];
        var audience = configuration["Auth0:Audience"];

        if (string.IsNullOrWhiteSpace(domain))
            throw new InvalidOperationException(
                "Auth0:Domain no está configurado. Revisa appsettings.json o las variables de entorno.");

        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException(
                "Auth0:Audience no está configurado. Revisa appsettings.json o las variables de entorno.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddAuthorization();
        services.AddScoped<IClaimsTransformation, RolesClaimsTransformation>();

        return services;
    }
}

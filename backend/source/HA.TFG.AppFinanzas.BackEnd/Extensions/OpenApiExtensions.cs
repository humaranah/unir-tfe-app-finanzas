using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace HA.TFG.AppFinanzas.BackEnd.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithAuth0(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info ??= new OpenApiInfo();
                document.Info.Title = "TFG App Finanzas API";
                document.Info.Version = "v1";
                document.Info.Description = "API para la aplicación de finanzas personales del TFG";

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Introduce el token JWT de Auth0"
                    };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IEndpointRouteBuilder MapOpenApiAndScalar(this IEndpointRouteBuilder app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "TFG App Finanzas API";
            options.Theme = ScalarTheme.DeepSpace;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecurityScheme = JwtBearerDefaults.AuthenticationScheme
            };
        });

        return app;
    }
}

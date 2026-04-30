namespace HA.TFG.AppFinanzas.BackEnd.Controllers.Requests;

/// <summary>
/// Datos de perfil enviados por la app tras el login en Auth0.
/// El IdAuth0 se extrae directamente del claim "sub" del JWT.
/// </summary>
public record EnsureUsuarioRequest(
    string Email,
    string Nombre,
    string? FotoPerfil,
    string? Proveedor,
    bool EmailVerificado,
    DateTimeOffset? UltimaActualizacion);

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public record EnsureUsuarioResult(
    long Id,
    string IdAuth0,
    string Email,
    string Nombre,
    string? FotoPerfil,
    string? Proveedor,
    bool EmailVerificado,
    DateTimeOffset? UltimaActualizacion,
    bool EsNuevo);

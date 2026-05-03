namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public record EnsureUsuarioResult(
    long Id,
    string Email,
    string Nombre,
    string? FotoPerfil,
    bool EmailVerificado,
    DateTimeOffset? UltimaActualizacion,
    bool EsNuevo);

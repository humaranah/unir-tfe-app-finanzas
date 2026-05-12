namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public record EnsureUsuarioResult(
    string Email,
    string Nombre,
    string? FotoPerfil,
    bool EmailVerificado,
    bool EsNuevo);

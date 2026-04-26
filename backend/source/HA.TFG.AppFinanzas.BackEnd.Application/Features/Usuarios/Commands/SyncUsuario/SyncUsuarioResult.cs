namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;

public record SyncUsuarioResult(
    long Id,
    string IdAuth0,
    string Email,
    string Nombre,
    bool EsNuevo);

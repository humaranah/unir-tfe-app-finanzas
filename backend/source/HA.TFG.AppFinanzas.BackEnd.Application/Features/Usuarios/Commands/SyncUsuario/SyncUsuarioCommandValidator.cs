using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.SyncUsuario;

public sealed class SyncUsuarioCommandValidator : AbstractValidator<SyncUsuarioCommand>
{
    public SyncUsuarioCommandValidator()
    {
        RuleFor(x => x.IdAuth0)
            .NotEmpty().WithMessage("El IdAuth0 es obligatorio.")
            .MaximumLength(100).WithMessage("El IdAuth0 no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(255).WithMessage("El email no puede superar los 255 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre no puede superar los 255 caracteres.");
    }
}

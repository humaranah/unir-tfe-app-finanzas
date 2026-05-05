using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Usuarios.Commands.EnsureUsuario;

public sealed class EnsureUsuarioCommandValidator : AbstractValidator<EnsureUsuarioCommand>
{
    public EnsureUsuarioCommandValidator()
    {
        RuleFor(x => x.IdAuth0)
            .NotEmpty().WithMessage("El IdAuth0 es obligatorio.")
            .MaximumLength(100).WithMessage("El IdAuth0 no puede superar los 100 caracteres.");

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("El AccessToken es obligatorio.");
    }
}

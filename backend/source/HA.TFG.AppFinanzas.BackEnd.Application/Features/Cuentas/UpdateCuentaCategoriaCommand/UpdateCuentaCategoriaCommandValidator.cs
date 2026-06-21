using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.UpdateCuentaCategoriaCommand;

public sealed class UpdateCuentaCategoriaCommandValidator : AbstractValidator<UpdateCuentaCategoriaCommand>
{
    public UpdateCuentaCategoriaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");
    }
}

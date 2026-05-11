using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public class CreateCuentaCommandValidator : AbstractValidator<CreateCuentaCommand>
{
    public CreateCuentaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda no puede estar vacía.")
            .MaximumLength(3).WithMessage("La moneda no puede tener más de 3 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción no puede estar vacía.")
            .MaximumLength(250).WithMessage("La descripción no puede tener más de 250 caracteres.");
    }
}

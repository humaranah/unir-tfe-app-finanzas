using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCommand;

public class CreateCuentaCommandValidator : AbstractValidator<CreateCuentaCommand>
{
    public CreateCuentaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda no puede estar vacía.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción no puede estar vacía.");
    }
}

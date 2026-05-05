using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.GetCuentasQuery;

public class GetCuentasQueryValidator : AbstractValidator<GetCuentasQuery>
{
    public GetCuentasQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");
    }
}

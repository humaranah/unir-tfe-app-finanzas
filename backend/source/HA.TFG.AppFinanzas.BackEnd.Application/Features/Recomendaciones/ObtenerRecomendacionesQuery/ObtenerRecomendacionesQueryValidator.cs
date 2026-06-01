using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Recomendaciones.ObtenerRecomendacionesQuery;

public class ObtenerRecomendacionesQueryValidator : AbstractValidator<ObtenerRecomendacionesQuery>
{
    public ObtenerRecomendacionesQueryValidator()
    {
        RuleFor(x => x.EmailUsuario)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.IdCuenta)
            .NotEmpty().WithMessage("El identificador de la cuenta no puede estar vacío.");

        RuleFor(x => x.Consulta)
            .MaximumLength(500).WithMessage("La consulta no puede superar los 500 caracteres.")
            .When(x => x.Consulta is not null);
    }
}

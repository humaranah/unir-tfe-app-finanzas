using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.DeleteMovimientoCommand;

public class DeleteMovimientoCommandValidator : AbstractValidator<DeleteMovimientoCommand>
{
    public DeleteMovimientoCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.");

        RuleFor(x => x.IdCuenta)
            .NotEmpty().WithMessage("El identificador de la cuenta es requerido.");

        RuleFor(x => x.IdMovimiento)
            .NotEmpty().WithMessage("El identificador del movimiento es requerido.");
    }
}

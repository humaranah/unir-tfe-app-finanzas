using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.UpdateMovimientoCommand;

public class UpdateMovimientoCommandValidator : AbstractValidator<UpdateMovimientoCommand>
{
    public UpdateMovimientoCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.");

        RuleFor(x => x.IdCuenta)
            .NotEmpty().WithMessage("El identificador de la cuenta es requerido.");

        RuleFor(x => x.IdMovimiento)
            .NotEmpty().WithMessage("El identificador del movimiento es requerido.");

        RuleFor(x => x.IdCuentaCategoria)
            .NotEmpty().WithMessage("El identificador de la categoría es requerido.");

        RuleFor(x => x.TipoMovimiento)
            .IsInEnum().WithMessage("El tipo de movimiento es inválido.");

        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto es requerido.")
            .MaximumLength(500).WithMessage("El concepto no puede exceder 500 caracteres.");

        RuleFor(x => x.Establecimiento)
            .MaximumLength(200).WithMessage("El establecimiento no puede exceder 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Establecimiento));

        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe tener 3 caracteres.");

        RuleFor(x => x.TipoCambio)
            .GreaterThan(0).WithMessage("El tipo de cambio debe ser mayor a cero.")
            .When(x => x.TipoCambio.HasValue);

        RuleFor(x => x.Nota)
            .NotNull().WithMessage("La nota no puede ser nula.")
            .MaximumLength(1000).WithMessage("La nota no puede exceder 1000 caracteres.");

        RuleFor(x => x.FechaMovimiento)
            .NotEmpty().WithMessage("La fecha del movimiento es requerida.");
    }
}

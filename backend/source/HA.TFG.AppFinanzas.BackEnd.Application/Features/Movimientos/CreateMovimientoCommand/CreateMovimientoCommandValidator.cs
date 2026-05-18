using FluentValidation;
using HA.TFG.AppFinanzas.BackEnd.Domain.ValueObjects;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Movimientos.CreateMovimientoCommand;

public class CreateMovimientoCommandValidator : AbstractValidator<CreateMovimientoCommand>
{
    public CreateMovimientoCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.IdCuenta)
            .NotEmpty().WithMessage("La cuenta es obligatoria.");

        RuleFor(x => x.IdCuentaCategoria)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.TipoMovimiento)
            .IsInEnum().WithMessage("El tipo de movimiento no es válido.");

        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto no puede estar vacío.")
            .MaximumLength(500).WithMessage("El concepto no puede superar los 500 caracteres.");

        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor que cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(3).WithMessage("La moneda no puede superar los 3 caracteres.");

        RuleFor(x => x.TipoCambio)
            .GreaterThan(0).When(x => x.TipoCambio.HasValue)
            .WithMessage("El tipo de cambio debe ser mayor que cero.");

        RuleFor(x => x.IdComprobante)
            .MaximumLength(100).When(x => x.IdComprobante is not null)
            .WithMessage("El identificador de comprobante no puede superar los 100 caracteres.");

        RuleFor(x => x.Nota)
            .MaximumLength(1000).WithMessage("La nota no puede superar los 1000 caracteres.");

        RuleFor(x => x.FechaMovimiento)
            .NotEmpty().WithMessage("La fecha del movimiento es obligatoria.");
    }
}

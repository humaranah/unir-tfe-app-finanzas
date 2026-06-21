using FluentValidation;

namespace HA.TFG.AppFinanzas.BackEnd.Application.Features.Cuentas.CreateCuentaCategoriaCommand;

public sealed class CreateCuentaCategoriaCommandValidator : AbstractValidator<CreateCuentaCategoriaCommand>
{
    public CreateCuentaCategoriaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.");

        RuleFor(x => x.IdCuenta)
             .NotEmpty().WithMessage("El identificador de la cuenta es requerido.");

        RuleFor(x => x.TipoMovimiento)
            .IsInEnum().WithMessage("El tipo de movimiento es inválido.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");
    }
}

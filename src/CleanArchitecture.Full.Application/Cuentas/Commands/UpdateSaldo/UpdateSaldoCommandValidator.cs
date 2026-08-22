// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateSaldo/UpdateSaldoCommandValidator.cs
using FluentValidation;
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateSaldo;

public class UpdateSaldoCommandValidator : AbstractValidator<UpdateSaldoCommand>
{
    public UpdateSaldoCommandValidator()
    {
        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto tiene que ser mayor a cero");

        RuleFor(x => x.TipoMovimiento)
            .NotEmpty().WithMessage("El tipo de movimiento es requerido")
            .Must(t => Enum.IsDefined(typeof(TipoMovimiento), t))
            .WithMessage("Tipo de movimiento inválido. Valores permitidos: Deposito, Retiro, Transferencia");
    }
}

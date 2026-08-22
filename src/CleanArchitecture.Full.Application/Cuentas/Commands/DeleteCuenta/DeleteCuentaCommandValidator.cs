// src/CleanArchitecture.Full.Application/Cuentas/Commands/DeleteCuenta/DeleteCuentaCommandValidator.cs
using FluentValidation;
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.DeleteCuenta;

public class DeleteCuentaCommandValidator : AbstractValidator<DeleteCuentaCommand>
{
    public DeleteCuentaCommandValidator()
    {
        RuleFor(x => x.Motivo)
            .Must(motivo => Enum.IsDefined(typeof(MotivoCancelacion), motivo!))
            .When(x => !string.IsNullOrEmpty(x.Motivo))
            .WithMessage("Motivo inválido. Valores permitidos: CierreVoluntario, Morosidad, Fraude");
    }
}

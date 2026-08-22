// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateCuenta/UpdateCuentaCommandValidator.cs
using FluentValidation;
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateCuenta;

public class UpdateCuentaCommandValidator : AbstractValidator<UpdateCuentaCommand>
{
    private static readonly string[] EstadosPermitidos =
    {
        nameof(EstadoCuenta.Activa),
        nameof(EstadoCuenta.Bloqueada)
    };

    public UpdateCuentaCommandValidator()
    {
        RuleFor(x => x.Estado)
            .Must(estado => EstadosPermitidos.Contains(estado))
            .When(x => !string.IsNullOrEmpty(x.Estado))
            .WithMessage("Estado inválido. Use Activa o Bloqueada; para cancelar una cuenta use DELETE /api/v1/cuentas/{id}");

        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimiteCredito.HasValue)
            .WithMessage("El límite de crédito no puede ser negativo");

        RuleFor(x => x.ComisionMantenimiento)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ComisionMantenimiento.HasValue)
            .WithMessage("La comisión de mantenimiento no puede ser negativa");
    }
}

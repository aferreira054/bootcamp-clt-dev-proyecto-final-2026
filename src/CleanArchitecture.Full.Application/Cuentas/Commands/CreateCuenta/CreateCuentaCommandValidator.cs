// src/CleanArchitecture.Full.Application/Cuentas/Commands/CreateCuenta/CreateCuentaCommandValidator.cs
using FluentValidation;
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.CreateCuenta;

public class CreateCuentaCommandValidator : AbstractValidator<CreateCuentaCommand>
{
    public CreateCuentaCommandValidator()
    {
        RuleFor(x => x.NumeroCuenta)
            .NotEmpty().WithMessage("El número de cuenta es requerido")
            .MinimumLength(10).WithMessage("El número de cuenta debe tener al menos 10 dígitos")
            .MaximumLength(20).WithMessage("El número de cuenta no puede exceder 20 dígitos")
            .Matches(@"^[0-9]+$").WithMessage("El número de cuenta solo debe contener dígitos");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de cuenta es requerido")
            .Must(t => Enum.IsDefined(typeof(TipoCuenta), t))
            .WithMessage("Tipo de cuenta inválido. Valores permitidos: Corriente, Ahorro, Credito");

        RuleFor(x => x.SaldoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo");

        RuleFor(x => x.Moneda)
            .Must(m => Enum.IsDefined(typeof(Moneda), m))
            .WithMessage("Moneda inválida. Valores permitidos: USD, EUR, MXN, PYG");

        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.ClienteNombre)
            .NotEmpty().WithMessage("El nombre del cliente es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.LimiteCredito)
            .NotNull().WithMessage("El límite de crédito es requerido para cuentas de crédito")
            .GreaterThan(0).WithMessage("El límite de crédito debe ser mayor a cero")
            .When(x => x.Tipo == "Credito");
    }
}
// tests/CleanArchitecture.Full.Application.Tests/Cuentas/UpdateSaldoCommandValidatorTests.cs
using CleanArchitecture.Full.Application.Cuentas.Commands.UpdateSaldo;

namespace CleanArchitecture.Full.Application.Tests.Cuentas;

public class UpdateSaldoCommandValidatorTests
{
    private readonly UpdateSaldoCommandValidator _validator = new();

    private static UpdateSaldoCommand ComandoValido() => new()
    {
        Id = Guid.NewGuid(),
        Monto = 100m,
        TipoMovimiento = "Deposito"
    };

    [Fact]
    public void Comando_valido_no_produce_errores()
    {
        var result = _validator.Validate(ComandoValido());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Monto_negativo_produce_error()
    {
        var command = ComandoValido();
        command.Monto = -50m;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSaldoCommand.Monto));
    }

    [Fact]
    public void Monto_cero_produce_error()
    {
        var command = ComandoValido();
        command.Monto = 0m;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSaldoCommand.Monto));
    }

    [Fact]
    public void TipoMovimiento_inexistente_produce_error()
    {
        var command = ComandoValido();
        command.TipoMovimiento = "Inexistente";

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSaldoCommand.TipoMovimiento));
    }
}

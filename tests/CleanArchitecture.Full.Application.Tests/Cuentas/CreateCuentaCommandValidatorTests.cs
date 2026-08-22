// tests/CleanArchitecture.Full.Application.Tests/Cuentas/CreateCuentaCommandValidatorTests.cs
using CleanArchitecture.Full.Application.Cuentas.Commands.CreateCuenta;

namespace CleanArchitecture.Full.Application.Tests.Cuentas;

public class CreateCuentaCommandValidatorTests
{
    private readonly CreateCuentaCommandValidator _validator = new();

    private static CreateCuentaCommand CuentaValida() => new()
    {
        NumeroCuenta = "1234567890",
        Tipo = "Corriente",
        SaldoInicial = 100m,
        Moneda = "USD",
        ClienteId = "cliente-1",
        ClienteNombre = "Juan Perez"
    };

    [Fact]
    public void Comando_valido_no_produce_errores()
    {
        var result = _validator.Validate(CuentaValida());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345678901234567890X")]
    [InlineData("ABCDEFGHIJ")]
    public void NumeroCuenta_invalido_produce_error(string numeroCuenta)
    {
        var command = CuentaValida();
        command.NumeroCuenta = numeroCuenta;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.NumeroCuenta));
    }

    [Fact]
    public void Tipo_inexistente_produce_error()
    {
        var command = CuentaValida();
        command.Tipo = "Inexistente";

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.Tipo));
    }

    [Fact]
    public void SaldoInicial_negativo_produce_error()
    {
        var command = CuentaValida();
        command.SaldoInicial = -1m;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.SaldoInicial));
    }

    [Fact]
    public void Cuenta_credito_sin_limite_produce_error()
    {
        var command = CuentaValida();
        command.Tipo = "Credito";
        command.LimiteCredito = null;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCuentaCommand.LimiteCredito));
    }

    [Fact]
    public void Cuenta_credito_con_limite_es_valida()
    {
        var command = CuentaValida();
        command.Tipo = "Credito";
        command.LimiteCredito = 5000m;

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}

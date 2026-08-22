// tests/CleanArchitecture.Full.Application.Tests/Domain/CuentaTests.cs
using CleanArchitecture.Full.Domain.Entities;
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Application.Tests.Domain;

public class CuentaTests
{
    private static Cuenta CrearCuenta(decimal saldoInicial = 100m) =>
        new("1234567890", TipoCuenta.Corriente, saldoInicial, Moneda.USD, "cliente-1", "Juan Perez");

    [Fact]
    public void Deposito_incrementa_el_saldo()
    {
        var cuenta = CrearCuenta(100m);

        cuenta.ActualizarSaldo(50m, TipoMovimiento.Deposito);

        Assert.Equal(150m, cuenta.Saldo);
        Assert.NotNull(cuenta.FechaUltimoMovimiento);
    }

    [Fact]
    public void Retiro_con_saldo_suficiente_decrementa_el_saldo()
    {
        var cuenta = CrearCuenta(100m);

        cuenta.ActualizarSaldo(40m, TipoMovimiento.Retiro);

        Assert.Equal(60m, cuenta.Saldo);
    }

    [Fact]
    public void Retiro_con_saldo_insuficiente_lanza_excepcion()
    {
        var cuenta = CrearCuenta(100m);

        Assert.Throws<InvalidOperationException>(() => cuenta.ActualizarSaldo(150m, TipoMovimiento.Retiro));
    }

    [Fact]
    public void No_se_puede_operar_una_cuenta_cancelada()
    {
        var cuenta = CrearCuenta(0m);
        cuenta.ActualizarEstado(EstadoCuenta.Cancelada, MotivoCancelacion.CierreVoluntario);

        Assert.Throws<InvalidOperationException>(() => cuenta.ActualizarSaldo(10m, TipoMovimiento.Deposito));
    }

    [Fact]
    public void No_se_puede_cancelar_una_cuenta_con_saldo_positivo()
    {
        var cuenta = CrearCuenta(100m);

        Assert.Throws<InvalidOperationException>(
            () => cuenta.ActualizarEstado(EstadoCuenta.Cancelada, MotivoCancelacion.CierreVoluntario));
    }

    [Fact]
    public void Cancelar_sin_motivo_lanza_excepcion()
    {
        var cuenta = CrearCuenta(0m);

        Assert.Throws<ArgumentException>(() => cuenta.ActualizarEstado(EstadoCuenta.Cancelada, motivo: null));
    }

    [Fact]
    public void Cancelar_con_saldo_cero_y_motivo_es_valido()
    {
        var cuenta = CrearCuenta(0m);

        cuenta.ActualizarEstado(EstadoCuenta.Cancelada, MotivoCancelacion.Morosidad);

        Assert.Equal(EstadoCuenta.Cancelada, cuenta.Estado);
        Assert.Equal(MotivoCancelacion.Morosidad, cuenta.MotivoCancelacion);
    }
}

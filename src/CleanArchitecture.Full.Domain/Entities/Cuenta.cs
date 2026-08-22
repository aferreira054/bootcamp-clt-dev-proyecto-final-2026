// src/CleanArchitecture.Full.Domain/Entities/Cuenta.cs
using CleanArchitecture.Full.Domain.Enums;

namespace CleanArchitecture.Full.Domain.Entities;

public class Cuenta
{
    public Guid Id { get; private set; }
    public string NumeroCuenta { get; private set; } = string.Empty;
    public TipoCuenta Tipo { get; private set; }
    public decimal Saldo { get; private set; }
    public Moneda Moneda { get; private set; }
    public EstadoCuenta Estado { get; private set; }
    public string ClienteId { get; private set; } = string.Empty;
    public string ClienteNombre { get; private set; } = string.Empty;
    public DateTime FechaApertura { get; private set; }
    public DateTime? FechaUltimoMovimiento { get; private set; }
    public decimal? LimiteCredito { get; private set; }
    public decimal ComisionMantenimiento { get; private set; }
    public MotivoCancelacion? MotivoCancelacion { get; private set; }

    private Cuenta() { } // Para EF Core

    public Cuenta(
        string numeroCuenta,
        TipoCuenta tipo,
        decimal saldoInicial,
        Moneda moneda,
        string clienteId,
        string clienteNombre,
        decimal? limiteCredito = null)
    {
        Id = Guid.NewGuid();
        NumeroCuenta = numeroCuenta;
        Tipo = tipo;
        Saldo = saldoInicial;
        Moneda = moneda;
        Estado = EstadoCuenta.Activa;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        FechaApertura = DateTime.UtcNow;
        ComisionMantenimiento = 5.50m;
        
        if (tipo == TipoCuenta.Credito && limiteCredito.HasValue)
            LimiteCredito = limiteCredito.Value;
    }

    // Métodos de dominio
    public void ActualizarSaldo(decimal monto, TipoMovimiento tipoMovimiento)
    {
        if (Estado == EstadoCuenta.Cancelada)
            throw new InvalidOperationException("No se puede operar con una cuenta cancelada");

        if (tipoMovimiento == TipoMovimiento.Retiro && Saldo - monto < 0)
            throw new InvalidOperationException("Saldo insuficiente");

        Saldo += (tipoMovimiento == TipoMovimiento.Deposito) ? monto : -monto;
        FechaUltimoMovimiento = DateTime.UtcNow;
    }

    public void ActualizarEstado(EstadoCuenta nuevoEstado, MotivoCancelacion? motivo = null)
    {
        if (nuevoEstado == EstadoCuenta.Cancelada)
        {
            if (Saldo > 0)
                throw new InvalidOperationException("No se puede cancelar una cuenta con saldo positivo");

            if (motivo is null)
                throw new ArgumentException("El motivo de cancelación es requerido", nameof(motivo));

            MotivoCancelacion = motivo;
        }

        Estado = nuevoEstado;
    }

    public void ActualizarLimiteCredito(decimal nuevoLimite)
    {
        if (Tipo != TipoCuenta.Credito)
            throw new InvalidOperationException("Solo cuentas de crédito tienen límite");

        LimiteCredito = nuevoLimite;
    }

    public void ActualizarComisionMantenimiento(decimal nuevaComision)
    {
        if (nuevaComision < 0)
            throw new InvalidOperationException("La comisión de mantenimiento no puede ser negativa");

        ComisionMantenimiento = nuevaComision;
    }
}
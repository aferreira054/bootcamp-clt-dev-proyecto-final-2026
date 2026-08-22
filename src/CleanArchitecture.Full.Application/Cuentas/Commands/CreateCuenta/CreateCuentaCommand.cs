// src/CleanArchitecture.Full.Application/Cuentas/Commands/CreateCuenta/CreateCuentaCommand.cs
using MediatR;
using CleanArchitecture.Full.Application.DTOs;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.CreateCuenta;

public class CreateCuentaCommand : IRequest<CuentaDetalleDto>
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public string Moneda { get; set; } = "USD";
    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal? LimiteCredito { get; set; }
}
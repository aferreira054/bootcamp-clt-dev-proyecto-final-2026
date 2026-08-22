// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateCuenta/UpdateCuentaCommand.cs
using MediatR;
using CleanArchitecture.Full.Application.DTOs;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateCuenta;

public class UpdateCuentaCommand : IRequest<CuentaDetalleDto?>
{
    public Guid Id { get; set; }
    public string? Estado { get; set; }
    public decimal? LimiteCredito { get; set; }
    public decimal? ComisionMantenimiento { get; set; }
}
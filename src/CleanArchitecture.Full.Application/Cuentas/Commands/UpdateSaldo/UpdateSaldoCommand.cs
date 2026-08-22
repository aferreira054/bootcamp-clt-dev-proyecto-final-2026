// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateSaldo/UpdateSaldoCommand.cs
using MediatR;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateSaldo;

public class UpdateSaldoCommand : IRequest<SaldoActualizadoResponse?>
{
    public Guid Id { get; set; }
    public decimal Monto { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
}

public class SaldoActualizadoResponse
{
    public decimal NuevoSaldo { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
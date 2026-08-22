// src/CleanArchitecture.Full.Application/Cuentas/Commands/DeleteCuenta/DeleteCuentaCommand.cs
using MediatR;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.DeleteCuenta;

public class DeleteCuentaCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string? Motivo { get; set; }
}
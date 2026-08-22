// src/CleanArchitecture.Full.Application/Cuentas/Commands/DeleteCuenta/DeleteCuentaCommandHandler.cs
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.DeleteCuenta;

public class DeleteCuentaCommandHandler : IRequestHandler<DeleteCuentaCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCuentaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCuentaCommand request, CancellationToken cancellationToken)
    {
        var cuenta = await _context.Cuentas
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (cuenta is null)
            return false;

        var motivo = string.IsNullOrEmpty(request.Motivo)
            ? MotivoCancelacion.CierreVoluntario
            : Enum.Parse<MotivoCancelacion>(request.Motivo);

        cuenta.ActualizarEstado(EstadoCuenta.Cancelada, motivo);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
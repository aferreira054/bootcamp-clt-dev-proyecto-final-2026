// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateSaldo/UpdateSaldoCommandHandler.cs
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateSaldo;

public class UpdateSaldoCommandHandler : IRequestHandler<UpdateSaldoCommand, SaldoActualizadoResponse?>
{
    private readonly IApplicationDbContext _context;

    public UpdateSaldoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoActualizadoResponse?> Handle(UpdateSaldoCommand request, CancellationToken cancellationToken)
    {
        var cuenta = await _context.Cuentas
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (cuenta is null)
            return null;

        var tipoMovimiento = Enum.Parse<TipoMovimiento>(request.TipoMovimiento);
        cuenta.ActualizarSaldo(request.Monto, tipoMovimiento);

        await _context.SaveChangesAsync(cancellationToken);

        return new SaldoActualizadoResponse
        {
            NuevoSaldo = cuenta.Saldo,
            FechaActualizacion = DateTime.UtcNow
        };
    }
}
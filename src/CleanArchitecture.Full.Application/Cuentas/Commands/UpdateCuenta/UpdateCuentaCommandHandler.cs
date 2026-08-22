// src/CleanArchitecture.Full.Application/Cuentas/Commands/UpdateCuenta/UpdateCuentaCommandHandler.cs
using AutoMapper;
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.UpdateCuenta;

public class UpdateCuentaCommandHandler : IRequestHandler<UpdateCuentaCommand, CuentaDetalleDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCuentaCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CuentaDetalleDto?> Handle(UpdateCuentaCommand request, CancellationToken cancellationToken)
    {
        var cuenta = await _context.Cuentas
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (cuenta is null)
            return null;

        if (!string.IsNullOrEmpty(request.Estado) && Enum.TryParse<EstadoCuenta>(request.Estado, out var estado))
        {
            cuenta.ActualizarEstado(estado);
        }

        if (request.LimiteCredito.HasValue)
        {
            cuenta.ActualizarLimiteCredito(request.LimiteCredito.Value);
        }

        if (request.ComisionMantenimiento.HasValue)
        {
            cuenta.ActualizarComisionMantenimiento(request.ComisionMantenimiento.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CuentaDetalleDto>(cuenta);
    }
}
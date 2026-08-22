// src/CleanArchitecture.Full.Application/Cuentas/Commands/CreateCuenta/CreateCuentaCommandHandler.cs
using AutoMapper;
using CleanArchitecture.Full.Application.Common.Exceptions;
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Domain.Entities;
using CleanArchitecture.Full.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Commands.CreateCuenta;

public class CreateCuentaCommandHandler : IRequestHandler<CreateCuentaCommand, CuentaDetalleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCuentaCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CuentaDetalleDto> Handle(CreateCuentaCommand request, CancellationToken cancellationToken)
    {
        var existe = await _context.Cuentas
            .AnyAsync(c => c.NumeroCuenta == request.NumeroCuenta, cancellationToken);
        
        if (existe)
            throw new ConflictException($"La cuenta {request.NumeroCuenta} ya existe");

        var cuenta = new Cuenta(
            request.NumeroCuenta,
            Enum.Parse<TipoCuenta>(request.Tipo),
            request.SaldoInicial,
            Enum.Parse<Moneda>(request.Moneda),
            request.ClienteId,
            request.ClienteNombre,
            request.LimiteCredito
        );

        await _context.Cuentas.AddAsync(cuenta, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CuentaDetalleDto>(cuenta);
    }
}
// src/CleanArchitecture.Full.Application/Cuentas/Queries/GetCuentaById/GetCuentaByIdQueryHandler.cs
using AutoMapper;
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentaById;

public class GetCuentaByIdQueryHandler : IRequestHandler<GetCuentaByIdQuery, CuentaDetalleDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCuentaByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CuentaDetalleDto?> Handle(GetCuentaByIdQuery request, CancellationToken cancellationToken)
    {
        var cuenta = await _context.Cuentas
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return cuenta is null ? null : _mapper.Map<CuentaDetalleDto>(cuenta);
    }
}
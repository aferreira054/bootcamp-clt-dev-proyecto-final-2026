// src/CleanArchitecture.Full.Application/Cuentas/Queries/GetCuentas/GetCuentasQueryHandler.cs
using AutoMapper;
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Application.DTOs.Responses;
using CleanArchitecture.Full.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentas;

public class GetCuentasQueryHandler : IRequestHandler<GetCuentasQuery, PaginacionResponse<CuentaResumenDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCuentasQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginacionResponse<CuentaResumenDto>> Handle(GetCuentasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cuentas.AsQueryable();

        if (!string.IsNullOrEmpty(request.Estado) && Enum.TryParse<EstadoCuenta>(request.Estado, out var estado))
        {
            query = query.Where(c => c.Estado == estado);
        }

        var total = await query.CountAsync(cancellationToken);

        var cuentas = await query
            .Skip(request.Offset)
            .Take(request.Limite)
            .ToListAsync(cancellationToken);

        return new PaginacionResponse<CuentaResumenDto>
        {
            Total = total,
            Limite = request.Limite,
            Offset = request.Offset,
            Datos = _mapper.Map<List<CuentaResumenDto>>(cuentas)
        };
    }
}
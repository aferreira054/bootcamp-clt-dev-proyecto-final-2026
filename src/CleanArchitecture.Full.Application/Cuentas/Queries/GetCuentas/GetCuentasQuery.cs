// src/CleanArchitecture.Full.Application/Cuentas/Queries/GetCuentas/GetCuentasQuery.cs
using MediatR;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Application.DTOs.Responses;

namespace CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentas;

public class GetCuentasQuery : IRequest<PaginacionResponse<CuentaResumenDto>>
{
    public int Limite { get; set; } = 10;
    public int Offset { get; set; } = 0;
    public string? Estado { get; set; }
}
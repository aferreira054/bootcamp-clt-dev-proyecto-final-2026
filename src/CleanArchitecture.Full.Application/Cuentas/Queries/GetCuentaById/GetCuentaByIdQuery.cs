// src/CleanArchitecture.Full.Application/Cuentas/Queries/GetCuentaById/GetCuentaByIdQuery.cs
using MediatR;
using CleanArchitecture.Full.Application.DTOs;

namespace CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentaById;

public class GetCuentaByIdQuery : IRequest<CuentaDetalleDto?>
{
    public Guid Id { get; set; }
}
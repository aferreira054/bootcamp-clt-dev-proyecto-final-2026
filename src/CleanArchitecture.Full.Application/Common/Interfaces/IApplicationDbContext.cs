// src/CleanArchitecture.Full.Application/Common/Interfaces/IApplicationDbContext.cs
using CleanArchitecture.Full.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Cuenta> Cuentas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
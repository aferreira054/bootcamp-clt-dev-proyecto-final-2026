// src/CleanArchitecture.Full.Infrastructure/Data/ApplicationDbContext.cs
using CleanArchitecture.Full.Application.Common.Interfaces;
using CleanArchitecture.Full.Domain.Entities;
using CleanArchitecture.Full.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cuenta> Cuentas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
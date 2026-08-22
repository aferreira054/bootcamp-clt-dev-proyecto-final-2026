// src/CleanArchitecture.Full.Infrastructure/Data/DbSeeder.cs
using CleanArchitecture.Full.Domain.Entities;
using CleanArchitecture.Full.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (await context.Cuentas.AnyAsync())
            return;

        var cuentas = new List<Cuenta>
        {
            new Cuenta("1234567890", TipoCuenta.Corriente, 1500.50m, Moneda.USD, "CLI-001", "Juan Pérez"),
            new Cuenta("0987654321", TipoCuenta.Ahorro, 5000.00m, Moneda.EUR, "CLI-002", "María García"),
            new Cuenta("1122334455", TipoCuenta.Credito, 0m, Moneda.MXN, "CLI-003", "Carlos López", 10000m),
            new Cuenta("6677889900", TipoCuenta.Credito, 30000m, Moneda.PYG, "CLI-004", "Alex Meza", 4000000m)
        };

        await context.Cuentas.AddRangeAsync(cuentas);
        await context.SaveChangesAsync();
    }
}
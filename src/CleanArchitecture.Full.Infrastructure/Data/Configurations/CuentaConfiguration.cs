using CleanArchitecture.Full.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Full.Infrastructure.Data.Configurations;

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("Cuentas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NumeroCuenta)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.NumeroCuenta)
            .IsUnique();

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Saldo)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Moneda)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Estado)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.ClienteId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ClienteNombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LimiteCredito)
            .HasPrecision(18, 2);

        builder.Property(c => c.ComisionMantenimiento)
            .HasPrecision(18, 2);

        builder.Property(c => c.MotivoCancelacion)
            .HasConversion<string>();
    }
}
// src/CleanArchitecture.Full.Application/DTOs/CuentaResumenDto.cs
namespace CleanArchitecture.Full.Application.DTOs;

public class CuentaResumenDto
{
    public Guid Id { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
}
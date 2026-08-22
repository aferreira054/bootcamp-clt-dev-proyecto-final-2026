// src/CleanArchitecture.Full.Application/DTOs/CuentaDetalleDto.cs
namespace CleanArchitecture.Full.Application.DTOs;

public class CuentaDetalleDto : CuentaResumenDto
{
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaUltimoMovimiento { get; set; }
    public string ClienteId { get; set; } = string.Empty;
    public decimal? LimiteCredito { get; set; }
    public decimal ComisionMantenimiento { get; set; }
    public string? MotivoCancelacion { get; set; }
}
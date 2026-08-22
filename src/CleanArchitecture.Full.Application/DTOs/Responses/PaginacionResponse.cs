// src/CleanArchitecture.Full.Application/DTOs/Responses/PaginacionResponse.cs
namespace CleanArchitecture.Full.Application.DTOs.Responses;

public class PaginacionResponse<T>
{
    public int Total { get; set; }
    public int Limite { get; set; }
    public int Offset { get; set; }
    public List<T> Datos { get; set; } = new();
}
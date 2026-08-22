// src/CleanArchitecture.Full.Application/Common/Exceptions/ConflictException.cs
namespace CleanArchitecture.Full.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

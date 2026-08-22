// src/CleanArchitecture.Full.Domain/Enums/EnumsCuenta.cs
namespace CleanArchitecture.Full.Domain.Enums;

public enum TipoCuenta
{
    Corriente = 1,
    Ahorro = 2,
    Credito = 3
}

public enum Moneda
{
    USD = 1,
    EUR = 2,
    MXN = 3,
    PYG = 4
}

public enum EstadoCuenta
{
    Activa = 1,
    Bloqueada = 2,
    Cancelada = 3
}

public enum TipoMovimiento
{
    Deposito = 1,
    Retiro = 2,
    Transferencia = 3
}

public enum MotivoCancelacion
{
    CierreVoluntario = 1,
    Morosidad = 2,
    Fraude = 3
}
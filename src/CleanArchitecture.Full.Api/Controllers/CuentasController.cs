// src/CleanArchitecture.Full.Api/Controllers/CuentasController.cs
using CleanArchitecture.Full.Application.Cuentas.Commands.CreateCuenta;
using CleanArchitecture.Full.Application.Cuentas.Commands.DeleteCuenta;
using CleanArchitecture.Full.Application.Cuentas.Commands.UpdateCuenta;
using CleanArchitecture.Full.Application.Cuentas.Commands.UpdateSaldo;
using CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentaById;
using CleanArchitecture.Full.Application.Cuentas.Queries.GetCuentas;
using CleanArchitecture.Full.Application.DTOs;
using CleanArchitecture.Full.Application.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Full.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CuentasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CuentasController> _logger;

    public CuentasController(IMediator mediator, ILogger<CuentasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cuentas con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginacionResponse<CuentaResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCuentas(
        [FromQuery] int limite = 10,
        [FromQuery] int offset = 0,
        [FromQuery] string? estado = null)
    {
        var query = new GetCuentasQuery
        {
            Limite = limite,
            Offset = offset,
            Estado = estado
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una cuenta por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CuentaDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCuentaById(Guid id)
    {
        var query = new GetCuentaByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result is null)
            return CuentaNoEncontrada(id);

        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva cuenta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CuentaDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCuenta([FromBody] CreateCuentaCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCuentaById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza una cuenta existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CuentaDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCuenta(
        Guid id,
        [FromBody] UpdateCuentaCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (result is null)
            return CuentaNoEncontrada(id);

        return Ok(result);
    }

    /// <summary>
    /// Elimina (baja lógica) una cuenta
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCuenta(
        Guid id,
        [FromQuery] string? motivo = null)
    {
        var command = new DeleteCuentaCommand { Id = id, Motivo = motivo };
        var result = await _mediator.Send(command);

        if (!result)
            return CuentaNoEncontrada(id);

        return NoContent();
    }

    /// <summary>
    /// Actualiza el saldo de una cuenta
    /// </summary>
    [HttpPatch("{id:guid}/saldo")]
    [ProducesResponseType(typeof(SaldoActualizadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSaldo(
        Guid id,
        [FromBody] UpdateSaldoCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);

        if (result is null)
            return CuentaNoEncontrada(id);

        return Ok(result);
    }

    private NotFoundObjectResult CuentaNoEncontrada(Guid id) => NotFound(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Recurso no encontrado",
        Detail = $"Cuenta con ID {id} no encontrada",
        Instance = HttpContext.Request.Path
    });
}

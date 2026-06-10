using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Tables.Commands.CreateTable;
using OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;
using OrderService.Application.UseCases.Tables.Queries.GetAllTables;
using OrderService.Application.UseCases.Tables.Queries.GetTableById;

namespace OrderService.Presentation.Controllers;

[ApiController]
[Route("api/v1/tables")]
public class TablesController : ControllerBase
{
    private readonly IGetAllTablesQueryHandler _getAllHandler;
    private readonly IGetTableByIdQueryHandler _getByIdHandler;
    private readonly ICreateTableCommandHandler _createHandler;
    private readonly IToggleTableStatusCommandHandler _toggleHandler;

    public TablesController(
        IGetAllTablesQueryHandler getAllHandler,
        IGetTableByIdQueryHandler getByIdHandler,
        ICreateTableCommandHandler createHandler,
        IToggleTableStatusCommandHandler toggleHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _toggleHandler = toggleHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TableResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TableResponseDto>>> GetAll(CancellationToken ct)
        => Ok(await _getAllHandler.Handle(new GetAllTablesQuery(), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _getByIdHandler.Handle(new GetTableByIdQuery { Id = id }, ct));

    [HttpPost]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TableResponseDto>> Create([FromBody] CreateTableCommand cmd, CancellationToken ct)
    {
        var result = await _createHandler.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> ToggleStatus(
        Guid id, [FromBody] ToggleTableStatusRequest req, CancellationToken ct)
        => Ok(await _toggleHandler.Handle(
            new ToggleTableStatusCommand { TableId = id, Enable = req.Enable }, ct));
}

public sealed class ToggleTableStatusRequest
{
    public bool Enable { get; init; }
}
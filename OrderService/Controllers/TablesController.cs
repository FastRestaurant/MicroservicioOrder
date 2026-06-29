using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Tables.Commands.CreateTable;
using OrderService.Application.UseCases.Tables.Commands.DeleteTable;
using OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;
using OrderService.Application.UseCases.Tables.Commands.UpdateTable;
using OrderService.Application.UseCases.Tables.Commands.UpdateTablePosition;
using OrderService.Application.UseCases.Tables.Queries.GetAllTables;
using OrderService.Application.UseCases.Tables.Queries.GetTableById;
using OrderService.Presentation.Authorization;

namespace OrderService.Presentation.Controllers;

[ApiController]
[Route("api/v1/tables")]
[Authorize]
public sealed class TablesController : ControllerBase
{
    private readonly IGetAllTablesQueryHandler _getAllHandler;
    private readonly IGetTableByIdQueryHandler _getByIdHandler;
    private readonly ICreateTableCommandHandler _createHandler;
    private readonly IDeleteTableCommandHandler _deleteHandler;
    private readonly IUpdateTableCommandHandler _updateHandler;
    private readonly IUpdateTablePositionCommandHandler _updatePositionHandler;
    private readonly IToggleTableStatusCommandHandler _toggleHandler;

    public TablesController(
        IGetAllTablesQueryHandler getAllHandler,
        IGetTableByIdQueryHandler getByIdHandler,
        ICreateTableCommandHandler createHandler,
        IDeleteTableCommandHandler deleteHandler,
        IUpdateTableCommandHandler updateHandler,
        IUpdateTablePositionCommandHandler updatePositionHandler,
        IToggleTableStatusCommandHandler toggleHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _updateHandler = updateHandler;
        _updatePositionHandler = updatePositionHandler;
        _toggleHandler = toggleHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<TableResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<TableResponseDto>>> GetAll([FromQuery] GetAllTablesQuery query, CancellationToken ct)
        => Ok(await _getAllHandler.Handle(query, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _getByIdHandler.Handle(new GetTableByIdQuery { Id = id }, ct));

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TableResponseDto>> Create([FromBody] CreateTableRequest req, CancellationToken ct)
    {
        var command = new CreateTableCommand
        {
            Number = req.Number,
            SeatCount = req.SeatCount,
            Location = req.Location,
            IsEnabled = req.IsEnabled
        };
        var result = await _createHandler.Handle(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> Update(Guid id, [FromBody] UpdateTableRequest req, CancellationToken ct)
        => Ok(await _updateHandler.Handle(new UpdateTableCommand
        {
            Id = id,
            Number = req.Number,
            SeatCount = req.SeatCount,
            Location = req.Location,
            IsEnabled = req.IsEnabled
        }, ct));

    [HttpPatch("{id:guid}/position")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> UpdatePosition(
        Guid id,
        [FromBody] UpdateTablePositionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _updatePositionHandler.Handle(new UpdateTablePositionCommand
        {
            TableId = id,
            PositionX = request.PositionX,
            PositionY = request.PositionY
        }, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deleteHandler.Handle(new DeleteTableCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(TableResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponseDto>> ToggleStatus(
        Guid id, [FromBody] ToggleTableStatusRequest req, CancellationToken ct)
        => Ok(await _toggleHandler.Handle(
            new ToggleTableStatusCommand { TableId = id, Enable = req.Enable }, ct));
}

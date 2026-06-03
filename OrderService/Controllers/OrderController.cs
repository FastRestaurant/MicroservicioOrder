using Microsoft.AspNetCore.Mvc;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Application.UseCases.Orders.Handlers;
using OrderService.Application.UseCases.Orders.Queries;

namespace OrderService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    CreateOrderHandler createOrderHandler,
    AddItemToOrderHandler addItemHandler,
    RemoveItemFromOrderHandler removeItemHandler,
    ChangeOrderStatusHandler changeStatusHandler,
    AddNoteToOrderHandler addNoteHandler,
    UpdateItemStatusHandler updateItemStatusHandler,
    GetOrderByIdHandler getByIdHandler,
    GetAllOrdersHandler getAllHandler,
    GetOrdersByStatusHandler getByStatusHandler,
    GetOrdersByTableHandler getByTableHandler
) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await getAllHandler.HandleAsync(new GetAllOrdersQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await getByIdHandler.HandleAsync(new GetOrderByIdQuery(id), ct));

    [HttpGet("{status}")]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
        => Ok(await getByStatusHandler.HandleAsync(new GetOrdersByStatusQuery(status), ct));

    [HttpGet("{tableNumber:int}")]
    public async Task<IActionResult> GetByTable(int tableNumber, CancellationToken ct)
        => Ok(await getByTableHandler.HandleAsync(new GetOrdersByTableQuery(tableNumber), ct));


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var result = await createOrderHandler.HandleAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemRequest req, CancellationToken ct)
    {
        var cmd = new AddItemToOrderCommand(id, req.ProductId, req.ProductType,
            req.ProductName, req.UnitPrice, req.Quantity, req.Notes);
        return Ok(await addItemHandler.HandleAsync(cmd, ct));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
        => Ok(await removeItemHandler.HandleAsync(new RemoveItemFromOrderCommand(id, itemId), ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
        => Ok(await changeStatusHandler.HandleAsync(new ChangeOrderStatusCommand(id, req.NewStatus, req.ChangedByUserId), ct));

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest req, CancellationToken ct)
        => Ok(await addNoteHandler.HandleAsync(new AddNoteToOrderCommand(id, req.CreatedByUserId, req.Note), ct));

    [HttpPatch("{id:guid}/items/{itemId:guid}/status")]
    public async Task<IActionResult> UpdateItemStatus(Guid id, Guid itemId, [FromBody] UpdateItemStatusRequest req, CancellationToken ct)
        => Ok(await updateItemStatusHandler.HandleAsync(new UpdateItemStatusCommand(id, itemId, req.NewStatus), ct));
}

public record AddItemRequest(Guid ProductId, string ProductType, string ProductName,
    decimal UnitPrice, int Quantity, string? Notes);
public record ChangeStatusRequest(string NewStatus, Guid ChangedByUserId);
public record AddNoteRequest(Guid CreatedByUserId, string Note);
public record UpdateItemStatusRequest(string NewStatus);

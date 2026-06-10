using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands.AddItemToOrder;
using OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;
using OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;
using OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;
using OrderService.Application.UseCases.Orders.Queries.GetAllOrders;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;
using OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetOrderStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

namespace OrderService.Presentation.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly ICreateOrderCommandHandler _createOrderHandler;
    private readonly IAddItemToOrderCommandHandler _addItemHandler;
    private readonly IRemoveItemFromOrderCommandHandler _removeItemHandler;
    private readonly IChangeOrderStatusCommandHandler _changeStatusHandler;
    private readonly IAddNoteToOrderCommandHandler _addNoteHandler;
    private readonly IUpdateItemStatusCommandHandler _updateItemStatusHandler;
    private readonly IGetAllOrdersQueryHandler _getAllHandler;
    private readonly IGetOrdersByStatusQueryHandler _getByStatusHandler;
    private readonly IGetOrderByIdQueryHandler _getByIdHandler;
    private readonly IGetOrdersByTableQueryHandler _getByTableHandler;
    private readonly IGetOrderStatusesQueryHandler _getOrderStatusesHandler;
    private readonly IGetOrderItemStatusesQueryHandler _getOrderItemStatusesHandler;

    public OrdersController(
        ICreateOrderCommandHandler createOrderHandler,
        IAddItemToOrderCommandHandler addItemHandler,
        IRemoveItemFromOrderCommandHandler removeItemHandler,
        IChangeOrderStatusCommandHandler changeStatusHandler,
        IAddNoteToOrderCommandHandler addNoteHandler,
        IUpdateItemStatusCommandHandler updateItemStatusHandler,
        IGetAllOrdersQueryHandler getAllHandler,
        IGetOrdersByStatusQueryHandler getByStatusHandler,
        IGetOrderByIdQueryHandler getByIdHandler,
        IGetOrdersByTableQueryHandler getByTableHandler,
        IGetOrderStatusesQueryHandler getOrderStatusesHandler,
        IGetOrderItemStatusesQueryHandler getOrderItemStatusesHandler)
    {
        _createOrderHandler = createOrderHandler;
        _addItemHandler = addItemHandler;
        _removeItemHandler = removeItemHandler;
        _changeStatusHandler = changeStatusHandler;
        _addNoteHandler = addNoteHandler;
        _updateItemStatusHandler = updateItemStatusHandler;
        _getAllHandler = getAllHandler;
        _getByStatusHandler = getByStatusHandler;
        _getByIdHandler = getByIdHandler;
        _getByTableHandler = getByTableHandler;
        _getOrderStatusesHandler = getOrderStatusesHandler;
        _getOrderItemStatusesHandler = getOrderItemStatusesHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<OrderSummaryDto>>> GetAll([FromQuery] GetAllOrdersQuery query, CancellationToken ct)
        => Ok(await _getAllHandler.Handle(query, ct));

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IEnumerable<StatusResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StatusResponseDto>>> GetOrderStatuses(CancellationToken ct)
        => Ok(await _getOrderStatusesHandler.Handle(new GetOrderStatusesQuery(), ct));

    [HttpGet("item-statuses")]
    [ProducesResponseType(typeof(IEnumerable<StatusResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StatusResponseDto>>> GetOrderItemStatuses(CancellationToken ct)
        => Ok(await _getOrderItemStatusesHandler.Handle(new GetOrderItemStatusesQuery(), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _getByIdHandler.Handle(new GetOrderByIdQuery { Id = id }, ct));

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetByStatus(string status, CancellationToken ct)
        => Ok(await _getByStatusHandler.Handle(new GetOrdersByStatusQuery { Status = status }, ct));

    [HttpGet("table/{tableId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetByTable(Guid tableId, CancellationToken ct)
     => Ok(await _getByTableHandler.Handle(new GetOrdersByTableQuery { TableId = tableId }, ct));
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var result = await _createOrderHandler.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> AddItem(Guid id, [FromBody] AddItemRequest req, CancellationToken ct)
    {
        var cmd = new AddItemToOrderCommand
        {
            OrderId = id,
            ProductId = req.ProductId,
            ProductType = req.ProductType,
            Quantity = req.Quantity,
            Notes = req.Notes
        };
        return Ok(await _addItemHandler.Handle(cmd, ct));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
        => Ok(await _removeItemHandler.Handle(new RemoveItemFromOrderCommand { OrderId = id, ItemId = itemId }, ct));

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
        => Ok(await _changeStatusHandler.Handle(new ChangeOrderStatusCommand { OrderId = id, NewStatus = req.NewStatus, ChangedByUserId = req.ChangedByUserId }, ct));

    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> AddNote(Guid id, [FromBody] AddNoteRequest req, CancellationToken ct)
        => Ok(await _addNoteHandler.Handle(new AddNoteToOrderCommand { OrderId = id, CreatedByUserId = req.CreatedByUserId, Note = req.Note }, ct));

    [HttpPatch("{id:guid}/items/{itemId:guid}/status")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> UpdateItemStatus(Guid id, Guid itemId, [FromBody] UpdateItemStatusRequest req, CancellationToken ct)
        => Ok(await _updateItemStatusHandler.Handle(new UpdateItemStatusCommand { OrderId = id, ItemId = itemId, NewStatus = req.NewStatus }, ct));

}

public sealed class AddItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

public sealed class ChangeStatusRequest
{
    public string NewStatus { get; init; } = string.Empty;
    public Guid ChangedByUserId { get; init; }
}

public sealed class AddNoteRequest
{
    public Guid CreatedByUserId { get; init; }
    public string Note { get; init; } = string.Empty;
}

public sealed class UpdateItemStatusRequest
{
    public string NewStatus { get; init; } = string.Empty;
}

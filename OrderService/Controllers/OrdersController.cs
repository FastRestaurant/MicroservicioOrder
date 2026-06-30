using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands.AddOrderItem;
using OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;
using OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Application.UseCases.Orders.Commands.NotifyOrderReadyByKitchen;
using OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;
using OrderService.Application.UseCases.Orders.Queries.GetActiveOrdersSummaryByTable;
using OrderService.Application.UseCases.Orders.Queries.GetAllOrders;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;
using OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetOrderStatuses;
using OrderService.Application.UseCases.Orders.Queries.GetReadyDeliveryOrders;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;
using OrderService.Domain.Exceptions;
using OrderService.Presentation.Authorization;
using System.Security.Claims;

namespace OrderService.Presentation.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ICreateOrderCommandHandler _createOrderHandler;
    private readonly IChangeOrderStatusCommandHandler _changeStatusHandler;
    private readonly INotifyOrderReadyByKitchenCommandHandler _notifyReadyByKitchenHandler;
    private readonly IAddOrderItemCommandHandler _addOrderItemHandler;
    private readonly IAddNoteToOrderCommandHandler _addNoteHandler;
    private readonly IUpdateItemStatusCommandHandler _updateItemStatusHandler;
    private readonly IGetAllOrdersQueryHandler _getAllHandler;
    private readonly IGetOrdersByStatusQueryHandler _getByStatusHandler;
    private readonly IGetOrderByIdQueryHandler _getByIdHandler;
    private readonly IGetOrdersByTableQueryHandler _getByTableHandler;
    private readonly IGetOrderStatusesQueryHandler _getOrderStatusesHandler;
    private readonly IGetOrderItemStatusesQueryHandler _getOrderItemStatusesHandler;
    private readonly IGetActiveOrdersSummaryByTableQueryHandler _getActiveSummaryByTableHandler;
    private readonly IGetReadyDeliveryOrdersQueryHandler _getReadyDeliveryOrdersHandler;

    public OrdersController(
        ICreateOrderCommandHandler createOrderHandler,
        IChangeOrderStatusCommandHandler changeStatusHandler,
        INotifyOrderReadyByKitchenCommandHandler notifyReadyByKitchenHandler,
        IAddOrderItemCommandHandler addOrderItemHandler,
        IAddNoteToOrderCommandHandler addNoteHandler,
        IUpdateItemStatusCommandHandler updateItemStatusHandler,
        IGetAllOrdersQueryHandler getAllHandler,
        IGetOrdersByStatusQueryHandler getByStatusHandler,
        IGetOrderByIdQueryHandler getByIdHandler,
        IGetOrdersByTableQueryHandler getByTableHandler,
        IGetOrderStatusesQueryHandler getOrderStatusesHandler,
        IGetOrderItemStatusesQueryHandler getOrderItemStatusesHandler,
        IGetActiveOrdersSummaryByTableQueryHandler getActiveSummaryByTableHandler,
        IGetReadyDeliveryOrdersQueryHandler getReadyDeliveryOrdersHandler)
    {
        _createOrderHandler = createOrderHandler;
        _changeStatusHandler = changeStatusHandler;
        _notifyReadyByKitchenHandler = notifyReadyByKitchenHandler;
        _addOrderItemHandler = addOrderItemHandler;
        _addNoteHandler = addNoteHandler;
        _updateItemStatusHandler = updateItemStatusHandler;
        _getAllHandler = getAllHandler;
        _getByStatusHandler = getByStatusHandler;
        _getByIdHandler = getByIdHandler;
        _getByTableHandler = getByTableHandler;
        _getOrderStatusesHandler = getOrderStatusesHandler;
        _getOrderItemStatusesHandler = getOrderItemStatusesHandler;
        _getActiveSummaryByTableHandler = getActiveSummaryByTableHandler;
        _getReadyDeliveryOrdersHandler = getReadyDeliveryOrdersHandler;
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
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
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _getByIdHandler.Handle(new GetOrderByIdQuery { Id = id }, ct));

    [HttpGet("status/{status}")]
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
    [ProducesResponseType(typeof(PagedResponseDto<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<OrderSummaryDto>>> GetByStatus(
        string status, [FromQuery] GetOrdersByStatusQuery query, CancellationToken ct)
        => Ok(await _getByStatusHandler.Handle(new GetOrdersByStatusQuery
        {
            Status = status,
            Page = query.Page,
            PageSize = query.PageSize
        }, ct));

    [HttpGet("table/{tableId:guid}")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(PagedResponseDto<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<OrderSummaryDto>>> GetByTable(
        Guid tableId, [FromQuery] GetOrdersByTableQuery query, CancellationToken ct)
     => Ok(await _getByTableHandler.Handle(new GetOrdersByTableQuery
     {
         TableId = tableId,
         Page = query.Page,
         PageSize = query.PageSize
     }, ct));

    [HttpGet("table/{tableId:guid}/active-summary")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(TableOrdersSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TableOrdersSummaryDto>> GetActiveSummaryByTable(Guid tableId, CancellationToken ct)
        => Ok(await _getActiveSummaryByTableHandler.Handle(new GetActiveOrdersSummaryByTableQuery { TableId = tableId }, ct));

    [HttpGet("ready-delivery")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<OrderResponseDto>>> GetReadyDelivery(CancellationToken ct)
        => Ok(await _getReadyDeliveryOrdersHandler.Handle(new GetReadyDeliveryOrdersQuery(), ct));

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var command = new CreateOrderCommand
        {
            TableId = req.TableId,
            WaiterId = GetCurrentUserId(),
            Items = req.Items.Select(item => new CreateOrderItemCommand
            {
                ProductId = item.ProductId,
                ProductType = item.ProductType,
                Quantity = item.Quantity,
                Notes = item.Notes
            }).ToArray()
        };

        var result = await _createOrderHandler.Handle(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
        => Ok(await _changeStatusHandler.Handle(new ChangeOrderStatusCommand { OrderId = id, NewStatus = req.NewStatus, ChangedByUserId = GetCurrentUserId() }, ct));

    [HttpPost("{id:guid}/kitchen-ready")]
    [Authorize(Roles = ApplicationRoles.AdminOrKitchen)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponseDto>> MarkReadyByKitchen(Guid id, [FromBody] KitchenReadyRequest? req, CancellationToken ct)
        => Ok(await _notifyReadyByKitchenHandler.Handle(new NotifyOrderReadyByKitchenCommand { OrderId = id, WasDelayed = req?.WasDelayed == true }, ct));

    [HttpPost("{id:guid}/notes")]
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> AddNote(Guid id, [FromBody] AddNoteRequest req, CancellationToken ct)
        => Ok(await _addNoteHandler.Handle(new AddNoteToOrderCommand { OrderId = id, CreatedByUserId = GetCurrentUserId(), Note = req.Note }, ct));

    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> AddItem(Guid id, [FromBody] CreateOrderItemRequest req, CancellationToken ct)
        => Ok(await _addOrderItemHandler.Handle(new AddOrderItemCommand
        {
            OrderId = id,
            RequestedByUserId = GetCurrentUserId(),
            ProductId = req.ProductId,
            ProductType = req.ProductType,
            Quantity = req.Quantity,
            Notes = req.Notes
        }, ct));

    [HttpPatch("{id:guid}/items/{itemId:guid}/status")]
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> UpdateItemStatus(Guid id, Guid itemId, [FromBody] UpdateItemStatusRequest req, CancellationToken ct)
        => Ok(await _updateItemStatusHandler.Handle(new UpdateItemStatusCommand { OrderId = id, ItemId = itemId, NewStatus = req.NewStatus }, ct));

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedException("El token no contiene un id de usuario valido.");

        return parsedUserId;
    }
}

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class UpdateItemStatusHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(UpdateItemStatusCommand cmd, CancellationToken ct = default)
    {
        var order = await repository.GetByIdWithDetailsAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        var item = order.Items.FirstOrDefault(i => i.Id == cmd.ItemId)
            ?? throw new NotFoundException(nameof(OrderItem), cmd.ItemId);

        item.UpdateStatus(cmd.NewStatus);
        await repository.SaveChangesAsync(ct);
        return order.ToResponseDto();
    }
}

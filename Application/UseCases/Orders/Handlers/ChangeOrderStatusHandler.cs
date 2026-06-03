using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

//public class ChangeOrderStatusHandler(IOrderRepository repository)
//{
//    public async Task<OrderResponseDto> HandleAsync(ChangeOrderStatusCommand cmd, CancellationToken ct = default)
//    {
//        var order = await repository.GetByIdWithDetailsAsync(cmd.OrderId, ct)
//            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

//        order.ChangeStatus(cmd.NewStatus, cmd.ChangedByUserId);
//        await repository.SaveChangesAsync(ct);
//        return order.ToResponseDto();
//    }
//}
public class ChangeOrderStatusHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(
     ChangeOrderStatusCommand cmd, CancellationToken ct = default)
    {
        var order = await repository.GetByIdWithDetailsAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        var history = order.ChangeStatus(cmd.NewStatus, cmd.ChangedByUserId);
        await repository.AddStatusHistoryAsync(history, ct);
        await repository.SaveChangesAsync(ct);
        return order.ToResponseDto();
    }
}
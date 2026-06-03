using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class CreateOrderHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(CreateOrderCommand cmd, CancellationToken ct = default)
    {
        var tableOccupied = await repository.HasActiveOrderForTableAsync(cmd.TableNumber, ct);
        if (tableOccupied)
            throw new DomainException(
                $"Table {cmd.TableNumber} already has an active order. Close it before opening a new one.");

        var order = Order.Create(cmd.TableNumber, cmd.WaiterId);
        await repository.AddAsync(order, ct);
        await repository.SaveChangesAsync(ct);
        await repository.ReloadAsync(order, ct);
        return order.ToResponseDto();
    }
}

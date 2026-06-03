using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class AddItemToOrderHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(AddItemToOrderCommand cmd, CancellationToken ct = default)
    {
        var order = await repository.GetByIdWithDetailsAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        var item = OrderItem.Create(
            cmd.OrderId, cmd.ProductId, cmd.ProductType,
            cmd.ProductName, cmd.UnitPrice, cmd.Quantity, cmd.Notes);

        await repository.AddItemAsync(item, ct);


        order.AddItem(item);
        await repository.SaveChangesAsync(ct);
        return order.ToResponseDto();
    }
}

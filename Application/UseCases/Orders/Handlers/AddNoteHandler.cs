using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Commands;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class AddNoteToOrderHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(AddNoteToOrderCommand cmd, CancellationToken ct = default)
    {
        var order = await repository.GetByIdWithDetailsAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        var note = order.AddNote(cmd.CreatedByUserId, cmd.Note);
        await repository.AddNoteAsync(note, ct);
        await repository.SaveChangesAsync(ct);
        return order.ToResponseDto();
    }
}

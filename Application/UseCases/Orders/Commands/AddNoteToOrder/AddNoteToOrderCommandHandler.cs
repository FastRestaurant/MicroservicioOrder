using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;

public sealed class AddNoteToOrderCommandHandler : IAddNoteToOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;

    public AddNoteToOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(AddNoteToOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.CreatedByUserId == Guid.Empty)
            throw new ValidationException("El id del usuario que crea la nota es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.Note))
            throw new ValidationException("La nota es obligatoria.");

        var userExists = await _userServiceClient.ExistsAsync(command.CreatedByUserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", command.CreatedByUserId);

        var order = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.AddNote(command.CreatedByUserId, command.Note);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        return OrderMapper.ToResponse(updatedOrder);
    }
}

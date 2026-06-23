using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandHandler : IChangeOrderStatusCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.NewStatus))
            throw new ValidationException("El estado solicitado es obligatorio.");

        if (command.ChangedByUserId == Guid.Empty)
            throw new ValidationException("El id del usuario que modifica el estado es obligatorio.");

        var userExists = await _userServiceClient.ExistsAsync(command.ChangedByUserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", command.ChangedByUserId);

        var newStatus = await _statusRepository.GetByNameAsync(command.NewStatus, StatusTypes.Order, cancellationToken)
            ?? throw new DomainException($"'{command.NewStatus}' no es un estado valido para una orden.");

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.ChangeStatus(newStatus.Id, command.ChangedByUserId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var updatedOrder = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        return OrderMapper.ToResponse(updatedOrder);
    }
}

using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;

public sealed class AddNoteToOrderCommandHandler : IAddNoteToOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<AddNoteToOrderCommandHandler> _logger;

    public AddNoteToOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<AddNoteToOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
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

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
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

        var updatedOrder = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var response = OrderMapper.ToResponse(updatedOrder);

        await NotifyOrderNoteAddedAsync(response, cancellationToken);

        return response;
    }

    private async Task NotifyOrderNoteAddedAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderNoteAddedAsync(order, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real la nota agregada a la orden {OrderId}.", order.Id);
        }
    }
}

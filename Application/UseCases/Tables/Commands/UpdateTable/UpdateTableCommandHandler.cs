using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Commands.UpdateTable;

public sealed class UpdateTableCommandHandler : IUpdateTableCommandHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTableCommandHandler(
        ITableRepository tableRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TableResponseDto> Handle(UpdateTableCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        var table = await _tableRepository.GetByIdForUpdateAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), command.Id);

        var number = command.Number?.Trim() ?? string.Empty;
        var location = command.Location?.Trim() ?? string.Empty;

        var existing = await _tableRepository.GetByNumberAsync(number, cancellationToken);
        if (existing is not null && existing.Id != command.Id)
            throw new DomainException($"Ya existe una mesa con el número '{number}'.");

        if (table.IsEnabled && !command.IsEnabled)
        {
            var hasActiveOrders = await _orderRepository.HasActiveOrdersForTableAsync(command.Id, cancellationToken);
            if (hasActiveOrders)
                throw new DomainException("No se puede deshabilitar una mesa con pedidos activos.");
        }

        table.Update(number, command.SeatCount, location, command.IsEnabled);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Id = table.Id,
            Number = table.Number,
            SeatCount = table.SeatCount,
            Location = table.Location,
            IsEnabled = table.IsEnabled,
            OperationalStatus = table.IsEnabled ? "Libre" : "Deshabilitada",
            PositionX = table.PositionX,
            PositionY = table.PositionY,
            Version = table.Version
        };
    }
}

using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Commands.DeleteTable;

public sealed class DeleteTableCommandHandler : IDeleteTableCommandHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTableCommandHandler(
        ITableRepository tableRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteTableCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        var table = await _tableRepository.GetByIdForUpdateAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), command.Id);

        var hasOrders = await _orderRepository.HasAnyOrderForTableAsync(command.Id, cancellationToken);
        if (hasOrders)
            throw new DomainException("No se puede eliminar una mesa con órdenes asociadas. Deshabilitala para ocultarla del uso operativo.");

        _tableRepository.Remove(table);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

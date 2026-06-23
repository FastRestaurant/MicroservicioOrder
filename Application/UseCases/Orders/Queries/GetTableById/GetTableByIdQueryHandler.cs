using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Queries.GetTableById;

public sealed class GetTableByIdQueryHandler : IGetTableByIdQueryHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IOrderRepository _orderRepository;

    public GetTableByIdQueryHandler(ITableRepository tableRepository, IOrderRepository orderRepository)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
    }

    public async Task<TableResponseDto> Handle(
        GetTableByIdQuery query, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), query.Id);

        return new TableResponseDto
        {
            Id = table.Id,
            Number = table.Number,
            SeatCount = table.SeatCount,
            Location = table.Location,
            IsEnabled = table.IsEnabled,
            OperationalStatus = await GetOperationalStatus(table, cancellationToken),
            Version = table.Version
        };
    }

    private async Task<string> GetOperationalStatus(Table table, CancellationToken cancellationToken)
    {
        if (!table.IsEnabled)
            return "Deshabilitada";

        var activeStatuses = await _orderRepository.GetActiveStatusNamesByTableIdsAsync([table.Id], cancellationToken);

        return activeStatuses.GetValueOrDefault(table.Id) switch
        {
            "ReadyToClose" => "Esperando",
            "Open" or "InProgress" => "Ocupada",
            _ => "Libre"
        };
    }
}

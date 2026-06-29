using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Queries.GetAllTables;

public sealed class GetAllTablesQueryHandler : IGetAllTablesQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly ITableRepository _tableRepository;
    private readonly IOrderRepository _orderRepository;

    public GetAllTablesQueryHandler(ITableRepository tableRepository, IOrderRepository orderRepository)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PagedResponseDto<TableResponseDto>> Handle(
        GetAllTablesQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
            throw new ValidationException("El número de página debe ser mayor a cero.");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            throw new ValidationException($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");

        var (tables, totalCount) = await _tableRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var activeStatusesByTableId = await _orderRepository.GetActiveStatusNamesByTableIdsAsync(
            tables.Select(table => table.Id), cancellationToken);
        var activeWaiterIdsByTableId = await _orderRepository.GetActiveWaiterIdsByTableIdsAsync(
            tables.Select(table => table.Id), cancellationToken);

        var tableDtos = tables.Select(table => Map(table, activeStatusesByTableId, activeWaiterIdsByTableId)).ToArray();

        return new PagedResponseDto<TableResponseDto>
        {
            Items = tableDtos,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
    }

    private static TableResponseDto Map(
        Table table,
        IReadOnlyDictionary<Guid, string> activeStatusesByTableId,
        IReadOnlyDictionary<Guid, Guid> activeWaiterIdsByTableId) => new()
    {
        Id = table.Id,
        Number = table.Number,
        SeatCount = table.SeatCount,
        Location = table.Location,
        IsEnabled = table.IsEnabled,
        OperationalStatus = GetOperationalStatus(table, activeStatusesByTableId),
        ActiveWaiterId = activeWaiterIdsByTableId.TryGetValue(table.Id, out var waiterId) ? waiterId : null,
        PositionX = table.PositionX,
        PositionY = table.PositionY,
        Version = table.Version
    };

    private static string GetOperationalStatus(Table table, IReadOnlyDictionary<Guid, string> activeStatusesByTableId)
    {
        if (!table.IsEnabled)
            return "Deshabilitada";

        return activeStatusesByTableId.GetValueOrDefault(table.Id) switch
        {
            "ReadyToClose" => "Esperando",
            "Open" or "InProgress" => "Ocupada",
            _ => "Libre"
        };
    }
}

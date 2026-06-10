using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.UseCases.Tables.Queries.GetAllTables;

public sealed class GetAllTablesQueryHandler : IGetAllTablesQueryHandler
{
    private readonly ITableRepository _tableRepository;

    public GetAllTablesQueryHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<IReadOnlyCollection<TableResponseDto>> Handle(
        GetAllTablesQuery query, CancellationToken cancellationToken = default)
    {
        var tables = await _tableRepository.GetAllAsync(cancellationToken);
        return tables.Select(Map).ToArray();
    }

    private static TableResponseDto Map(Table t) => new()
    {
        Id = t.Id,
        Number = t.Number,
        Status = t.Status
    };
}
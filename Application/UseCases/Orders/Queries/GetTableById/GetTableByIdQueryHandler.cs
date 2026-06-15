using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Queries.GetTableById;

public sealed class GetTableByIdQueryHandler : IGetTableByIdQueryHandler
{
    private readonly ITableRepository _tableRepository;

    public GetTableByIdQueryHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
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
            Status = table.Status
        };
    }
}
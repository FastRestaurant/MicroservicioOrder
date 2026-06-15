using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Queries.GetAllTables;

namespace OrderService.Application.Interfaces;

public interface IGetAllTablesQueryHandler
{
    Task<PagedResponseDto<TableResponseDto>> Handle(GetAllTablesQuery query, CancellationToken cancellationToken = default);
}

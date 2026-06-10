using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Queries.GetTableById;

namespace OrderService.Application.Interfaces;

public interface IGetTableByIdQueryHandler 
{
    Task<TableResponseDto> Handle(GetTableByIdQuery query, CancellationToken cancellationToken = default);
}
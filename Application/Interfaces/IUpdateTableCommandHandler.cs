using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Commands.UpdateTable;

namespace OrderService.Application.Interfaces;

public interface IUpdateTableCommandHandler
{
    Task<TableResponseDto> Handle(UpdateTableCommand command, CancellationToken cancellationToken = default);
}

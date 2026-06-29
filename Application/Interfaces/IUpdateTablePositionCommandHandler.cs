using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Commands.UpdateTablePosition;

namespace OrderService.Application.Interfaces;

public interface IUpdateTablePositionCommandHandler
{
    Task<TableResponseDto> Handle(
        UpdateTablePositionCommand command,
        CancellationToken cancellationToken = default);
}

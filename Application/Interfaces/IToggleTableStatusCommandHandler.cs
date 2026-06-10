using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;

namespace OrderService.Application.Interfaces;

public interface IToggleTableStatusCommandHandler
{
    Task<TableResponseDto> Handle(ToggleTableStatusCommand command, CancellationToken cancellationToken = default);
}
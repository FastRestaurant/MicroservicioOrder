using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Tables.Commands.CreateTable;

namespace OrderService.Application.Interfaces;

public interface ICreateTableCommandHandler
{
    Task<TableResponseDto> Handle(CreateTableCommand command, CancellationToken cancellationToken = default);
}
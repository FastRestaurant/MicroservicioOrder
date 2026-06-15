using OrderService.Application.UseCases.Tables.Commands.DeleteTable;

namespace OrderService.Application.Interfaces;

public interface IDeleteTableCommandHandler
{
    Task Handle(DeleteTableCommand command, CancellationToken cancellationToken = default);
}

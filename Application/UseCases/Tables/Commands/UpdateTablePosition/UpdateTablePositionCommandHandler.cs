using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Commands.UpdateTablePosition;

public sealed class UpdateTablePositionCommandHandler : IUpdateTablePositionCommandHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTablePositionCommandHandler(ITableRepository tableRepository, IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TableResponseDto> Handle(
        UpdateTablePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        var table = await _tableRepository.GetByIdAsync(command.TableId, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), command.TableId);

        table.UpdatePosition(command.PositionX, command.PositionY);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Id = table.Id,
            Number = table.Number,
            SeatCount = table.SeatCount,
            Location = table.Location,
            IsEnabled = table.IsEnabled,
            OperationalStatus = table.IsEnabled ? "Libre" : "Deshabilitada",
            PositionX = table.PositionX,
            PositionY = table.PositionY,
            Version = table.Version
        };
    }
}

using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Commands.UpdateTable;

public sealed class UpdateTableCommandHandler : IUpdateTableCommandHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTableCommandHandler(ITableRepository tableRepository, IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TableResponseDto> Handle(UpdateTableCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        var table = await _tableRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), command.Id);

        var number = command.Number.Trim();
        var location = command.Location.Trim();

        var existing = await _tableRepository.GetByNumberAsync(number, cancellationToken);
        if (existing is not null && existing.Id != command.Id)
            throw new DomainException($"Ya existe una mesa con el número '{number}'.");

        table.Update(number, command.SeatCount, location, command.IsEnabled);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Id = table.Id,
            Number = table.Number,
            SeatCount = table.SeatCount,
            Location = table.Location,
            IsEnabled = table.IsEnabled,
            OperationalStatus = table.IsEnabled ? "Libre" : "Deshabilitada",
            Version = table.Version
        };
    }
}

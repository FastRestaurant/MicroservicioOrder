using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Tables.Commands.CreateTable;

public sealed class CreateTableCommandHandler : ICreateTableCommandHandler
{
    private readonly ITableRepository _tableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTableCommandHandler(ITableRepository tableRepository, IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TableResponseDto> Handle(
        CreateTableCommand command, CancellationToken cancellationToken = default)
    {
        var number = command.Number.Trim();
        var location = command.Location.Trim();

        var table = Table.Create(number, command.SeatCount, location, command.IsEnabled);

        var existing = await _tableRepository.GetByNumberAsync(number, cancellationToken);
        if (existing is not null)
            throw new DomainException($"Ya existe una mesa con el número '{number}'.");

        await _tableRepository.AddAsync(table, cancellationToken);
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

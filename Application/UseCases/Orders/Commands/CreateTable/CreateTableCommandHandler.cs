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
        if (string.IsNullOrWhiteSpace(command.Number))
            throw new ValidationException("El número de mesa es obligatorio.");

        var existing = await _tableRepository.GetByNumberAsync(command.Number, cancellationToken);
        if (existing is not null)
            throw new DomainException($"Ya existe una mesa con el número '{command.Number}'.");

        var table = Table.Create(command.Number);
        await _tableRepository.AddAsync(table, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Id = table.Id,
            Number = table.Number,
            Status = table.Status
        };
    }
}
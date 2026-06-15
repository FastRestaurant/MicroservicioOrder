using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Table?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Table>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Table>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Table table, CancellationToken cancellationToken = default);
}
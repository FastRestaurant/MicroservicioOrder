using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface ITableRepository
{
    Task<Table?> GetByIdForReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Table?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Table?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Table>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Table> Tables, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Table>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Table table, CancellationToken cancellationToken = default);
    void Remove(Table table);
}

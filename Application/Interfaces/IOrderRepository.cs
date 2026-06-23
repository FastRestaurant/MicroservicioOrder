using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdForReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedByStatusAsync(int statusId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedByTableAsync(Guid tableId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> HasActiveOrderForTableAsync(Guid tableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetActiveStatusNamesByTableIdsAsync(IEnumerable<Guid> tableIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Guid>> GetActiveWaiterIdsByTableIdsAsync(IEnumerable<Guid> tableIds, CancellationToken cancellationToken = default);
    Task<bool> HasAnyOrderForTableAsync(Guid tableId, CancellationToken cancellationToken = default);
}

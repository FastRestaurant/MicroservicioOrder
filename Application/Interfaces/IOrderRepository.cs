using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(int statusId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByTableAsync(int tableNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> HasActiveOrderForTableAsync(int tableNumber, CancellationToken cancellationToken = default);
}

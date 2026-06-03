using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByTableAsync(int tableNumber, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task ReloadAsync(Order order, CancellationToken ct = default);
    Task AddItemAsync(OrderItem item, CancellationToken ct = default);
    Task AddNoteAsync(OrderNote note, CancellationToken ct = default);
    Task AddStatusHistoryAsync(OrderStatusHistory history, CancellationToken ct = default);
    Task<bool> HasActiveOrderForTableAsync(int tableNumber, CancellationToken ct = default);
    void DetachAll();

}

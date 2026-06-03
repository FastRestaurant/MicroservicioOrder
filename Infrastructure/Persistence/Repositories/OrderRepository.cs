using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Orders.FindAsync([id], ct);

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .Include(o => o.Notes)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByTableAsync(int tableNumber, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .Where(o => o.TableNumber == tableNumber)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await context.Orders.AddAsync(order, ct);
      
    }

    public async Task AddItemAsync(OrderItem item, CancellationToken ct = default)
       => await context.OrderItems.AddAsync(item, ct);

    public async Task AddNoteAsync(OrderNote note, CancellationToken ct = default)
    => await context.OrderNotes.AddAsync(note, ct);


    public async Task SaveChangesAsync(CancellationToken ct = default)
    => await context.SaveChangesAsync(ct);

    public async Task ReloadAsync(Order order, CancellationToken ct = default)
    => await context.Entry(order).ReloadAsync(ct);

    public void DetachAll()
    {
        foreach (var entry in context.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }
    public async Task AddStatusHistoryAsync(OrderStatusHistory history, CancellationToken ct = default)
    {
        await context.OrderStatusHistories.AddAsync(history, ct);
    }

    public async Task<bool> HasActiveOrderForTableAsync(int tableNumber, CancellationToken ct = default)
    {
        var activeStatuses = new[]
        {
        OrderStatus.Open,
        OrderStatus.InProgress,
        OrderStatus.ReadyToClose
        };

        return await context.Orders
            .AnyAsync(o => o.TableNumber == tableNumber
                        && activeStatuses.Contains(o.Status), ct);
    }
}

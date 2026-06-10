using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
                .ThenInclude(i => i.Status)
            .Include(o => o.Notes)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.PreviousStatus)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.NewStatus)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (orders, totalCount);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(int statusId, CancellationToken ct = default)
        => await _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .Where(o => o.StatusId == statusId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByTableAsync(Guid tableId, CancellationToken ct = default)
        => await _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .Where(o => o.TableId == tableId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _context.Orders.AddAsync(order, ct);

    public async Task<bool> HasActiveOrderForTableAsync(Guid tableId, CancellationToken ct = default)
    {
        var activeStatuses = new[]
        {
            OrderStatusIds.Open,
            OrderStatusIds.InProgress,
            OrderStatusIds.ReadyToClose
        };

        return await _context.Orders
            .AnyAsync(o => o.TableId == tableId
                        && activeStatuses.Contains(o.StatusId), ct);
    }
}
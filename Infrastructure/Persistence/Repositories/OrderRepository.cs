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

    public Task<Order?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => WithDetails(_context.Orders)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByIdForReadAsync(Guid id, CancellationToken ct = default)
        => WithDetails(_context.Orders)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    private static IQueryable<Order> WithDetails(IQueryable<Order> orders)
        => orders
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
                .ThenInclude(i => i.Status)
            .Include(o => o.Notes)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.PreviousStatus)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.NewStatus);

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

    public async Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedByStatusAsync(
        int statusId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .Where(o => o.StatusId == statusId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (orders, totalCount);
    }

    public async Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetPagedByTableAsync(
        Guid tableId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .Where(o => o.TableId == tableId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (orders, totalCount);
    }

    public async Task<IReadOnlyCollection<Order>> GetActiveByTableAsync(Guid tableId, CancellationToken ct = default)
    {
        var activeStatuses = new[]
        {
            OrderStatusIds.Open,
            OrderStatusIds.InProgress,
            OrderStatusIds.ReadyToClose
        };

        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Status)
            .Include(o => o.Items)
            .Where(o => o.TableId == tableId && activeStatuses.Contains(o.StatusId))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Order>> GetActiveWithReadyItemsAsync(CancellationToken ct = default)
    {
        var activeStatuses = new[]
        {
            OrderStatusIds.Open,
            OrderStatusIds.InProgress,
            OrderStatusIds.ReadyToClose
        };

        return await WithDetails(_context.Orders)
            .AsNoTracking()
            .AsSplitQuery()
            .Where(o => activeStatuses.Contains(o.StatusId) &&
                        o.Items.Any(i => i.StatusId == OrderItemStatusIds.Ready))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _context.Orders.AddAsync(order, ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetActiveStatusNamesByTableIdsAsync(
        IEnumerable<Guid> tableIds, CancellationToken ct = default)
    {
        var ids = tableIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<Guid, string>();

        var activeStatuses = new[]
        {
            OrderStatusIds.Open,
            OrderStatusIds.InProgress,
            OrderStatusIds.ReadyToClose
        };

        var activeOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Status)
            .Where(o => ids.Contains(o.TableId) && activeStatuses.Contains(o.StatusId))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.TableId, StatusName = o.Status.Name })
            .ToListAsync(ct);

        return activeOrders
            .GroupBy(order => order.TableId)
            .ToDictionary(
                group => group.Key,
                group => group.Any(order => order.StatusName is "Open" or "InProgress")
                    ? "InProgress"
                    : "ReadyToClose");
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> GetActiveWaiterIdsByTableIdsAsync(
        IEnumerable<Guid> tableIds, CancellationToken ct = default)
    {
        var ids = tableIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<Guid, Guid>();

        var activeStatuses = new[]
        {
            OrderStatusIds.Open,
            OrderStatusIds.InProgress,
            OrderStatusIds.ReadyToClose
        };

        var activeOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => ids.Contains(o.TableId) && activeStatuses.Contains(o.StatusId))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.TableId, o.WaiterId })
            .ToListAsync(ct);

        return activeOrders
            .GroupBy(order => order.TableId)
            .ToDictionary(group => group.Key, group => group.First().WaiterId);
    }

    public async Task<bool> HasAnyOrderForTableAsync(Guid tableId, CancellationToken ct = default)
        => await _context.Orders.AnyAsync(o => o.TableId == tableId, ct);
}

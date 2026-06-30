using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class TableRepository : ITableRepository
{
    private readonly AppDbContext _context;

    public TableRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Table?> GetByIdForReadAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Table?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Tables
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Table?> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
        => _context.Tables
            .FirstOrDefaultAsync(t => t.Number == number, cancellationToken);

    public async Task<IReadOnlyCollection<Table>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Tables
            .AsNoTracking()
            .OrderBy(t => t.Number.Length)
            .ThenBy(t => t.Number)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyCollection<Table> Tables, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Tables
            .AsNoTracking()
            .OrderBy(t => t.Number.Length)
            .ThenBy(t => t.Number);

        var totalCount = await query.CountAsync(cancellationToken);
        var tables = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (tables, totalCount);
    }

    public async Task<IReadOnlyCollection<Table>> GetAvailableAsync(CancellationToken cancellationToken = default)
        => await _context.Tables
            .AsNoTracking()
            .Where(t => t.IsEnabled)
            .OrderBy(t => t.Number.Length)
            .ThenBy(t => t.Number)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Table table, CancellationToken cancellationToken = default)
        => await _context.Tables.AddAsync(table, cancellationToken);

    public void Remove(Table table)
        => _context.Tables.Remove(table);
}

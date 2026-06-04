using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class StatusRepository : IStatusRepository
{
    private readonly AppDbContext _context;

    public StatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Status?> GetByNameAsync(string name, string type, CancellationToken cancellationToken = default)
    {
        return _context.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name && s.Type == type, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Status>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await _context.Statuses
            .AsNoTracking()
            .Where(s => s.Type == type)
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }
}

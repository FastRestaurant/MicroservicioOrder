using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IStatusRepository
{
    Task<Status?> GetByNameAsync(string name, string type, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Status>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
}

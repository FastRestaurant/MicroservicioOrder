namespace OrderService.Application.Interfaces;

public interface IUserServiceClient
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

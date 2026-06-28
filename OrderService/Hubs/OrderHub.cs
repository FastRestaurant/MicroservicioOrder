using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderService.Presentation.Authorization;

namespace OrderService.Presentation.Hubs;

[Authorize]
public sealed class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var groups = ResolveGroupsForCurrentUser();

        if (groups.Count == 0)
            _logger.LogWarning(
                "La conexion {ConnectionId} del usuario {UserIdentifier} no resolvio ningun grupo de rol y no recibira eventos en tiempo real.",
                Context.ConnectionId,
                Context.UserIdentifier);

        foreach (var group in groups)
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

        await base.OnConnectedAsync();
    }

    private List<string> ResolveGroupsForCurrentUser()
    {
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet() ?? [];
        var groups = new List<string>();

        if (roles.Contains(ApplicationRoles.Admin))
        {
            groups.Add(OrderHubGroups.Admin);
            groups.Add(OrderHubGroups.Waitress);
            groups.Add(OrderHubGroups.Kitchen);
            return groups;
        }

        if (roles.Contains(ApplicationRoles.Waitress))
            groups.Add(OrderHubGroups.Waitress);

        if (roles.Contains(ApplicationRoles.Kitchen))
            groups.Add(OrderHubGroups.Kitchen);

        return groups;
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OrderService.Presentation.Authorization;

namespace OrderService.Presentation.Hubs;

[Authorize]
public sealed class OrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        foreach (var group in ResolveGroupsForCurrentUser())
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var group in ResolveGroupsForCurrentUser())
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

        await base.OnDisconnectedAsync(exception);
    }

    private IEnumerable<string> ResolveGroupsForCurrentUser()
    {
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet() ?? [];

        if (roles.Contains(ApplicationRoles.Admin))
        {
            yield return OrderHubGroups.Admin;
            yield return OrderHubGroups.Waitress;
            yield return OrderHubGroups.Kitchen;
            yield break;
        }

        if (roles.Contains(ApplicationRoles.Waitress))
            yield return OrderHubGroups.Waitress;

        if (roles.Contains(ApplicationRoles.Kitchen))
            yield return OrderHubGroups.Kitchen;
    }
}

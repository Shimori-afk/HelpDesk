using HelpDesk.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public const string AgentsGroup = "Agents";

    public override async Task OnConnectedAsync()
    {
        var roleClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, out var role) && role is UserRole.Agent or UserRole.Admin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AgentsGroup);
        }

        await base.OnConnectedAsync();
    }
}

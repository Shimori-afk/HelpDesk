using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Hubs;
using HelpDesk.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Services;

public class NotificationDispatcher(ApplicationDbContext db, IHubContext<NotificationHub> hub) : INotificationDispatcher
{
    public async Task NotifyUserAsync(int userId, int? ticketId, string message, NotificationType type, DateTimeOffset now)
    {
        var notification = new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            Message = message,
            Type = type,
            CreatedAt = now
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        var dto = new NotificationDto(notification.Id, notification.TicketId, notification.Message, notification.Type, notification.IsRead, notification.CreatedAt);
        await hub.Clients.User(userId.ToString()).SendAsync("notificationReceived", dto);
    }

    public async Task BroadcastTicketCreatedAsync(TicketDetailDto ticket)
    {
        await hub.Clients.Group(NotificationHub.AgentsGroup).SendAsync("ticketCreated", ticket);
    }
}

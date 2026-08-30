using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Services;

public interface INotificationDispatcher
{
    Task NotifyUserAsync(int userId, int? ticketId, string message, NotificationType type, DateTimeOffset now);
    Task BroadcastTicketCreatedAsync(TicketDetailDto ticket);
}

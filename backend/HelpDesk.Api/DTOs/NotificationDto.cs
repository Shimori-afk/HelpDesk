using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs;

public record NotificationDto(
    int Id,
    int? TicketId,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTimeOffset CreatedAt);

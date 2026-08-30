using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public record TicketActivityDto(
    int Id,
    int UserId,
    string UserName,
    TicketActivityType Type,
    string? Description,
    string? OldValue,
    string? NewValue,
    DateTimeOffset CreatedAt);

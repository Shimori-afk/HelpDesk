using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public record TicketListItemDto(
    int Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    string CategoryName,
    string? DepartmentName,
    int RequesterId,
    string RequesterName,
    int? AssignedAgentId,
    string? AssignedAgentName,
    DateTimeOffset CreatedAt,
    DateTimeOffset ResponseDueAt,
    DateTimeOffset ResolutionDueAt,
    DateTimeOffset? ResolvedAt,
    SlaState SlaState);

using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public record TicketDetailDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    string? Location,
    int CategoryId,
    string CategoryName,
    int? DepartmentId,
    string? DepartmentName,
    int RequesterId,
    string RequesterName,
    int? AssignedAgentId,
    string? AssignedAgentName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset ResponseDueAt,
    DateTimeOffset ResolutionDueAt,
    DateTimeOffset? RespondedAt,
    IReadOnlyList<TicketCommentDto> Comments,
    IReadOnlyList<TicketActivityDto> Activities,
    IReadOnlyList<AttachmentDto> Attachments);

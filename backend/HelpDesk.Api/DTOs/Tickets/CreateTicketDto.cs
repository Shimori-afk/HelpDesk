using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public record CreateTicketDto(
    string Title,
    string Description,
    int CategoryId,
    TicketPriority Priority,
    int? DepartmentId,
    string? Location);

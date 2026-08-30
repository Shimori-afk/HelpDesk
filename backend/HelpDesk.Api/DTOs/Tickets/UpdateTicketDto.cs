namespace HelpDesk.Api.DTOs.Tickets;

public record UpdateTicketDto(
    string Title,
    string Description,
    int CategoryId,
    int? DepartmentId,
    string? Location);

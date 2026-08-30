namespace HelpDesk.Api.DTOs.Tickets;

public record TicketCommentDto(int Id, int AuthorId, string AuthorName, string Message, DateTimeOffset CreatedAt);

public record CreateCommentDto(string Message);

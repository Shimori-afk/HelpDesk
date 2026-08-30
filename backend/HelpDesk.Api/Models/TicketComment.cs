namespace HelpDesk.Api.Models;

public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

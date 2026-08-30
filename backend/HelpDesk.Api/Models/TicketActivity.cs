namespace HelpDesk.Api.Models;

public class TicketActivity
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public TicketActivityType Type { get; set; }
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

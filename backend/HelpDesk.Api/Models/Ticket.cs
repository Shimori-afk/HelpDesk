namespace HelpDesk.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; }
    public string? Location { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public int? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset ResponseDueAt { get; set; }
    public DateTimeOffset ResolutionDueAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketActivity> Activities { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}

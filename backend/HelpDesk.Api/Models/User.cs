namespace HelpDesk.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Ticket> TicketsCreated { get; set; } = [];
    public ICollection<Ticket> AssignedTickets { get; set; } = [];
    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketActivity> Activities { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Attachment> UploadedAttachments { get; set; } = [];
}

namespace HelpDesk.Api.Models;

public enum UserRole
{
    Employee,
    Agent,
    Admin
}

public enum UserStatus
{
    Active,
    Inactive
}

public enum TicketStatus
{
    Open,
    InProgress,
    WaitingForUser,
    Escalated,
    Resolved,
    Closed,
    Reopened
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketActivityType
{
    Created,
    Assigned,
    StatusChanged,
    PriorityChanged,
    CommentAdded,
    Escalated,
    Resolved,
    Reopened,
    Closed
}

public enum NotificationType
{
    TicketAssigned,
    PriorityChanged,
    StatusChanged,
    CommentAdded,
    TicketEscalated,
    TicketResolved
}

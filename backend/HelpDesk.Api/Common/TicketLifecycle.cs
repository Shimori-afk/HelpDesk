using HelpDesk.Api.Models;

namespace HelpDesk.Api.Common;

public static class TicketLifecycle
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] = [TicketStatus.InProgress, TicketStatus.Escalated, TicketStatus.Closed],
        [TicketStatus.InProgress] = [TicketStatus.WaitingForUser, TicketStatus.Resolved, TicketStatus.Escalated],
        [TicketStatus.WaitingForUser] = [TicketStatus.InProgress, TicketStatus.Resolved, TicketStatus.Escalated],
        [TicketStatus.Escalated] = [TicketStatus.InProgress, TicketStatus.Resolved],
        [TicketStatus.Resolved] = [TicketStatus.Closed, TicketStatus.Reopened],
        [TicketStatus.Reopened] = [TicketStatus.InProgress],
        [TicketStatus.Closed] = []
    };

    public static bool CanTransition(TicketStatus from, TicketStatus to) =>
        from == to || (AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to));

    public static bool RequiresAgentOrAdmin(TicketStatus to) => to != TicketStatus.Reopened;
}

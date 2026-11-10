using HelpDesk.Api.Models;

namespace HelpDesk.Api.Common;

public static class SlaCalculator
{
    // Once less than this fraction of the resolution window remains, the ticket is flagged as
    // approaching breach.
    private const double ApproachingThresholdFraction = 0.2;

    public static SlaState ComputeState(TicketStatus status, DateTimeOffset createdAt, DateTimeOffset resolutionDueAt, DateTimeOffset? resolvedAt, DateTimeOffset now)
    {
        var referenceTime = resolvedAt ?? now;
        if (referenceTime > resolutionDueAt)
        {
            return SlaState.Breached;
        }

        if (status is TicketStatus.Resolved or TicketStatus.Closed)
        {
            return SlaState.WithinSla;
        }

        var totalWindow = resolutionDueAt - createdAt;
        if (totalWindow <= TimeSpan.Zero)
        {
            return SlaState.Breached;
        }

        var remainingFraction = (resolutionDueAt - now) / totalWindow;
        return remainingFraction <= ApproachingThresholdFraction ? SlaState.ApproachingBreach : SlaState.WithinSla;
    }

    public static SlaState ComputeState(Ticket ticket, DateTimeOffset now) =>
        ComputeState(ticket.Status, ticket.CreatedAt, ticket.ResolutionDueAt, ticket.ResolvedAt, now);
}

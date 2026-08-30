using HelpDesk.Api.Common;
using HelpDesk.Api.Data;
using HelpDesk.Api.Models;
using HelpDesk.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.BackgroundServices;

// Runs on a timer, finds tickets approaching or breaching their SLA, notifies the relevant
// agent/admins, and auto-escalates any ticket that breaches. Each ticket is notified for a given
// SLA event (approaching / breached) at most once, tracked via the Notifications table itself
// rather than a separate "already notified" column.
public class SlaMonitoringService(IServiceScopeFactory scopeFactory, ILogger<SlaMonitoringService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private static readonly TicketStatus[] OpenStatuses =
    [
        TicketStatus.Open, TicketStatus.InProgress, TicketStatus.WaitingForUser,
        TicketStatus.Escalated, TicketStatus.Reopened
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await CheckSlaAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "SLA monitoring pass failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckSlaAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var now = DateTimeOffset.UtcNow;

        var tickets = await db.Tickets
            .Where(t => OpenStatuses.Contains(t.Status))
            .ToListAsync(ct);

        if (tickets.Count == 0)
        {
            return;
        }

        var adminIds = await db.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).ToListAsync(ct);

        foreach (var ticket in tickets)
        {
            var state = SlaCalculator.ComputeState(ticket, now);
            if (state == SlaState.WithinSla)
            {
                continue;
            }

            var notificationType = state == SlaState.Breached ? NotificationType.SlaBreached : NotificationType.SlaApproaching;

            var alreadyNotified = await db.Notifications
                .AnyAsync(n => n.TicketId == ticket.Id && n.Type == notificationType, ct);
            if (alreadyNotified)
            {
                continue;
            }

            var recipientIds = new HashSet<int>(adminIds);
            if (ticket.AssignedAgentId is not null)
            {
                recipientIds.Add(ticket.AssignedAgentId.Value);
            }

            var verb = state == SlaState.Breached ? "breached its SLA" : "is approaching its SLA deadline";
            foreach (var recipientId in recipientIds)
            {
                await notifier.NotifyUserAsync(recipientId, ticket.Id, $"Ticket \"{ticket.Title}\" {verb}", notificationType, now);
            }

            var canAutoEscalate = state == SlaState.Breached
                && ticket.Status != TicketStatus.Escalated
                && TicketLifecycle.CanTransition(ticket.Status, TicketStatus.Escalated);

            if (canAutoEscalate)
            {
                var oldStatus = ticket.Status;
                ticket.Status = TicketStatus.Escalated;
                ticket.UpdatedAt = now;

                db.TicketActivities.Add(new TicketActivity
                {
                    TicketId = ticket.Id,
                    UserId = null,
                    Type = TicketActivityType.Escalated,
                    Description = $"Ticket auto-escalated after breaching its SLA ({oldStatus} → {TicketStatus.Escalated})",
                    OldValue = oldStatus.ToString(),
                    NewValue = TicketStatus.Escalated.ToString(),
                    CreatedAt = now
                });

                await db.SaveChangesAsync(ct);
            }
        }
    }
}

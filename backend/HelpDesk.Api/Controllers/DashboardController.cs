using HelpDesk.Api.Common;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(ApplicationDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    private static readonly TicketStatus[] OpenStatuses =
    [
        TicketStatus.Open, TicketStatus.InProgress, TicketStatus.WaitingForUser,
        TicketStatus.Escalated, TicketStatus.Reopened
    ];

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var tickets = db.Tickets.AsQueryable();
        if (currentUser.Role == UserRole.Employee)
        {
            tickets = tickets.Where(t => t.CreatedById == currentUser.UserId);
        }

        var statusCounts = await tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountFor(TicketStatus status) => statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;

        var now = DateTimeOffset.UtcNow;
        var myAssignedCount = await tickets.CountAsync(t => t.AssignedAgentId == currentUser.UserId && OpenStatuses.Contains(t.Status));
        var unassignedCount = await tickets.CountAsync(t => t.AssignedAgentId == null && OpenStatuses.Contains(t.Status));
        var overdueCount = await tickets.CountAsync(t => OpenStatuses.Contains(t.Status) && t.ResolutionDueAt < now);
        var criticalOpenCount = await tickets.CountAsync(t => OpenStatuses.Contains(t.Status) && t.Priority == TicketPriority.Critical);

        return Ok(new DashboardSummaryDto(
            CountFor(TicketStatus.Open),
            CountFor(TicketStatus.InProgress),
            CountFor(TicketStatus.WaitingForUser),
            CountFor(TicketStatus.Escalated),
            CountFor(TicketStatus.Resolved),
            CountFor(TicketStatus.Closed),
            CountFor(TicketStatus.Reopened),
            myAssignedCount,
            unassignedCount,
            overdueCount,
            criticalOpenCount));
    }
}

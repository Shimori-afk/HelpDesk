using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs.Reports;
using HelpDesk.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/reports")]
public class ReportsController(ApplicationDbContext db) : ControllerBase
{
    private const int TicketsPerDayWindowDays = 30;

    [HttpGet("summary")]
    public async Task<ActionResult<ReportsSummaryDto>> GetSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = DateTimeOffset.UtcNow.AddDays(-(TicketsPerDayWindowDays - 1)).Date;

        var recentCreatedDates = await db.Tickets
            .Where(t => t.CreatedAt >= cutoff)
            .Select(t => t.CreatedAt)
            .ToListAsync();

        var countsByDate = recentCreatedDates
            .GroupBy(d => DateOnly.FromDateTime(d.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        var ticketsPerDay = Enumerable.Range(0, TicketsPerDayWindowDays)
            .Select(offset => today.AddDays(-(TicketsPerDayWindowDays - 1 - offset)))
            .Select(date => new DailyCountDto(date, countsByDate.GetValueOrDefault(date)))
            .ToList();

        var categoryCounts = await db.Tickets
            .GroupBy(t => t.Category.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();
        var byCategory = categoryCounts
            .OrderByDescending(c => c.Count)
            .Select(c => new NamedCountDto(c.Name, c.Count))
            .ToList();

        var priorityCounts = await db.Tickets
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();
        var byPriority = priorityCounts
            .OrderBy(p => p.Priority)
            .Select(p => new NamedCountDto(p.Priority.ToString(), p.Count))
            .ToList();

        var statusCounts = await db.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        var byStatus = statusCounts
            .OrderBy(s => s.Status)
            .Select(s => new NamedCountDto(s.Status.ToString(), s.Count))
            .ToList();

        var agentCounts = await db.Tickets
            .Where(t => t.AssignedAgentId != null)
            .GroupBy(t => t.AssignedAgent!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();
        var byAgent = agentCounts
            .OrderByDescending(a => a.Count)
            .Select(a => new NamedCountDto(a.Name, a.Count))
            .ToList();

        var resolvedTickets = await db.Tickets
            .Where(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.ResolvedAt != null)
            .Select(t => new { t.CreatedAt, t.ResolvedAt, t.ResolutionDueAt })
            .ToListAsync();

        double? averageResolutionHours = resolvedTickets.Count == 0
            ? null
            : resolvedTickets.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);

        var slaComplianceRate = resolvedTickets.Count == 0
            ? 100d
            : resolvedTickets.Count(t => t.ResolvedAt!.Value <= t.ResolutionDueAt) * 100d / resolvedTickets.Count;

        return Ok(new ReportsSummaryDto(
            ticketsPerDay, byCategory, byPriority, byStatus, byAgent,
            averageResolutionHours, Math.Round(slaComplianceRate, 1)));
    }
}

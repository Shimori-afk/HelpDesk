using HelpDesk.Api.Common;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Tests;

public class SlaCalculatorTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Due = Created.AddHours(10);

    [Fact]
    public void Open_ticket_with_plenty_of_time_left_is_within_sla()
    {
        var now = Created.AddHours(1);

        var state = SlaCalculator.ComputeState(TicketStatus.Open, Created, Due, resolvedAt: null, now);

        Assert.Equal(SlaState.WithinSla, state);
    }

    [Fact]
    public void Open_ticket_with_less_than_20_percent_of_window_left_is_approaching_breach()
    {
        // 10-hour window; 1 hour (< 20%) remaining.
        var now = Due.AddHours(-1);

        var state = SlaCalculator.ComputeState(TicketStatus.InProgress, Created, Due, resolvedAt: null, now);

        Assert.Equal(SlaState.ApproachingBreach, state);
    }

    [Fact]
    public void Open_ticket_past_its_due_date_is_breached()
    {
        var now = Due.AddMinutes(1);

        var state = SlaCalculator.ComputeState(TicketStatus.InProgress, Created, Due, resolvedAt: null, now);

        Assert.Equal(SlaState.Breached, state);
    }

    [Fact]
    public void Resolved_ticket_before_its_due_date_is_within_sla_regardless_of_current_time()
    {
        var resolvedAt = Due.AddHours(-2);
        var now = Due.AddDays(5); // long after resolution; must not affect the verdict

        var state = SlaCalculator.ComputeState(TicketStatus.Resolved, Created, Due, resolvedAt, now);

        Assert.Equal(SlaState.WithinSla, state);
    }

    [Fact]
    public void Resolved_ticket_after_its_due_date_is_breached()
    {
        var resolvedAt = Due.AddHours(1);
        var now = Due.AddDays(5);

        var state = SlaCalculator.ComputeState(TicketStatus.Resolved, Created, Due, resolvedAt, now);

        Assert.Equal(SlaState.Breached, state);
    }

    [Fact]
    public void Closed_ticket_after_its_due_date_is_breached()
    {
        var resolvedAt = Due.AddHours(1);
        var now = Due.AddDays(5);

        var state = SlaCalculator.ComputeState(TicketStatus.Closed, Created, Due, resolvedAt, now);

        Assert.Equal(SlaState.Breached, state);
    }
}

using HelpDesk.Api.Common;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Tests;

public class TicketLifecycleTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Open, TicketStatus.Escalated)]
    [InlineData(TicketStatus.Open, TicketStatus.Closed)]
    [InlineData(TicketStatus.InProgress, TicketStatus.WaitingForUser)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Escalated)]
    [InlineData(TicketStatus.WaitingForUser, TicketStatus.InProgress)]
    [InlineData(TicketStatus.WaitingForUser, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Escalated, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Escalated, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Reopened)]
    [InlineData(TicketStatus.Reopened, TicketStatus.InProgress)]
    public void CanTransition_allows_documented_transitions(TicketStatus from, TicketStatus to)
    {
        Assert.True(TicketLifecycle.CanTransition(from, to));
    }

    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Open, TicketStatus.Reopened)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Closed)]
    [InlineData(TicketStatus.WaitingForUser, TicketStatus.Closed)]
    [InlineData(TicketStatus.Escalated, TicketStatus.Open)]
    [InlineData(TicketStatus.Escalated, TicketStatus.Closed)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Reopened, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Reopened, TicketStatus.Closed)]
    public void CanTransition_rejects_undocumented_transitions(TicketStatus from, TicketStatus to)
    {
        Assert.False(TicketLifecycle.CanTransition(from, to));
    }

    [Fact]
    public void CanTransition_from_Closed_is_always_false_except_to_itself()
    {
        foreach (var status in Enum.GetValues<TicketStatus>())
        {
            if (status == TicketStatus.Closed)
            {
                Assert.True(TicketLifecycle.CanTransition(TicketStatus.Closed, status));
            }
            else
            {
                Assert.False(TicketLifecycle.CanTransition(TicketStatus.Closed, status));
            }
        }
    }

    [Theory]
    [InlineData(TicketStatus.Open)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.WaitingForUser)]
    [InlineData(TicketStatus.Escalated)]
    [InlineData(TicketStatus.Resolved)]
    [InlineData(TicketStatus.Closed)]
    [InlineData(TicketStatus.Reopened)]
    public void CanTransition_a_status_to_itself_is_always_allowed(TicketStatus status)
    {
        Assert.True(TicketLifecycle.CanTransition(status, status));
    }
}

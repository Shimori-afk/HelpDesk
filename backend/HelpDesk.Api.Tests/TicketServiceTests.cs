using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Models;
using HelpDesk.Api.Services;
using HelpDesk.Api.Tests.TestHelpers;
using Moq;

namespace HelpDesk.Api.Tests;

public class TicketServiceTests
{
    private const int EmployeeId = 1;
    private const int AgentId = 2;
    private const int AdminId = 3;
    private const int OtherEmployeeId = 4;

    private static TicketService CreateService(Data.ApplicationDbContext db, int userId, UserRole role, Mock<INotificationDispatcher>? notifier = null) =>
        new(db, new FakeCurrentUserService(userId, role), (notifier ?? new Mock<INotificationDispatcher>()).Object);

    private static Mock<INotificationDispatcher> CreateNotifierMock()
    {
        var mock = new Mock<INotificationDispatcher>();
        mock.Setup(n => n.NotifyUserAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<DateTimeOffset>()))
            .Returns(Task.CompletedTask);
        mock.Setup(n => n.BroadcastTicketCreatedAsync(It.IsAny<TicketDetailDto>())).Returns(Task.CompletedTask);
        return mock;
    }

    [Fact]
    public async Task CreateTicketAsync_sets_sla_due_dates_from_the_matching_priority_policy()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var service = CreateService(db, EmployeeId, UserRole.Employee, CreateNotifierMock());

        var before = DateTimeOffset.UtcNow;
        var ticket = await service.CreateTicketAsync(new CreateTicketDto("Broken monitor", "Flickers constantly", 1, TicketPriority.High, null, null));
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.InRange(ticket.ResponseDueAt, before.AddMinutes(60), after.AddMinutes(60));
        Assert.InRange(ticket.ResolutionDueAt, before.AddMinutes(480), after.AddMinutes(480));
    }

    [Fact]
    public async Task CreateTicketAsync_rejects_an_unknown_category()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var service = CreateService(db, EmployeeId, UserRole.Employee);

        await Assert.ThrowsAsync<BadRequestAppException>(() =>
            service.CreateTicketAsync(new CreateTicketDto("Title", "Description", 999, TicketPriority.Low, null, null)));
    }

    [Fact]
    public async Task GetTicketByIdAsync_hides_another_employees_ticket_as_not_found()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var owner = CreateService(db, EmployeeId, UserRole.Employee, CreateNotifierMock());
        var created = await owner.CreateTicketAsync(new CreateTicketDto("Private issue", "Only I should see this", 1, TicketPriority.Low, null, null));

        var otherEmployee = CreateService(db, OtherEmployeeId, UserRole.Employee);

        await Assert.ThrowsAsync<NotFoundException>(() => otherEmployee.GetTicketByIdAsync(created.Id));
    }

    [Fact]
    public async Task GetTicketByIdAsync_lets_agents_view_any_ticket()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var owner = CreateService(db, EmployeeId, UserRole.Employee, CreateNotifierMock());
        var created = await owner.CreateTicketAsync(new CreateTicketDto("Visible to agents", "Description", 1, TicketPriority.Low, null, null));

        var agent = CreateService(db, AgentId, UserRole.Agent);
        var fetched = await agent.GetTicketByIdAsync(created.Id);

        Assert.Equal(created.Id, fetched.Id);
    }

    [Fact]
    public async Task ChangeStatusAsync_rejects_a_transition_not_in_the_lifecycle_map()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var agent = CreateService(db, AgentId, UserRole.Agent, CreateNotifierMock());
        var created = await agent.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        // Open -> Resolved is not an allowed direct transition.
        await Assert.ThrowsAsync<BadRequestAppException>(() => agent.ChangeStatusAsync(created.Id, TicketStatus.Resolved));
    }

    [Fact]
    public async Task ChangeStatusAsync_forbids_an_employee_from_changing_status_on_their_own_ticket()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        await Assert.ThrowsAsync<ForbiddenAppException>(() => employee.ChangeStatusAsync(created.Id, TicketStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatusAsync_lets_an_employee_reopen_their_own_resolved_ticket()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        var agent = CreateService(db, AgentId, UserRole.Agent, notifier);
        await agent.ChangeStatusAsync(created.Id, TicketStatus.InProgress);
        await agent.ChangeStatusAsync(created.Id, TicketStatus.Resolved);

        var reopened = await employee.ChangeStatusAsync(created.Id, TicketStatus.Reopened);

        Assert.Equal(TicketStatus.Reopened, reopened.Status);
        Assert.Null(reopened.ResolvedAt);
    }

    [Fact]
    public async Task AssignAsync_forbids_employees_from_assigning_tickets()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        await Assert.ThrowsAsync<ForbiddenAppException>(() => employee.AssignAsync(created.Id, AgentId));
    }

    [Fact]
    public async Task AssignAsync_moves_an_open_ticket_to_in_progress_once_an_agent_is_assigned()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        var admin = CreateService(db, AdminId, UserRole.Admin, notifier);
        var assigned = await admin.AssignAsync(created.Id, AgentId);

        Assert.Equal(TicketStatus.InProgress, assigned.Status);
        Assert.Equal(AgentId, assigned.AssignedAgentId);
    }

    [Fact]
    public async Task ChangePriorityAsync_to_critical_automatically_escalates_the_ticket()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        var agent = CreateService(db, AgentId, UserRole.Agent, notifier);
        var updated = await agent.ChangePriorityAsync(created.Id, TicketPriority.Critical);

        Assert.Equal(TicketStatus.Escalated, updated.Status);
        Assert.Equal(TicketPriority.Critical, updated.Priority);
    }

    [Fact]
    public async Task EscalateAsync_rejects_escalating_a_closed_ticket()
    {
        await using var db = await DbContextFactory.CreateSeededDbAsync();
        var notifier = CreateNotifierMock();
        var employee = CreateService(db, EmployeeId, UserRole.Employee, notifier);
        var created = await employee.CreateTicketAsync(new CreateTicketDto("Title", "Description", 1, TicketPriority.Low, null, null));

        var admin = CreateService(db, AdminId, UserRole.Admin, notifier);
        await admin.ChangeStatusAsync(created.Id, TicketStatus.Closed);

        await Assert.ThrowsAsync<BadRequestAppException>(() => admin.EscalateAsync(created.Id));
    }
}

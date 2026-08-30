using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var departments = new[]
        {
            new Department { Name = "IT" },
            new Department { Name = "HR" },
            new Department { Name = "Finance" },
            new Department { Name = "Security" },
            new Department { Name = "Operations" },
            new Department { Name = "Management" }
        };
        db.Departments.AddRange(departments);

        var categories = new[]
        {
            new Category { Name = "Hardware" },
            new Category { Name = "Software" },
            new Category { Name = "Network" },
            new Category { Name = "Account Access" },
            new Category { Name = "Security" },
            new Category { Name = "Email" },
            new Category { Name = "Printer" },
            new Category { Name = "Other" }
        };
        db.Categories.AddRange(categories);

        var slaPolicies = new[]
        {
            new SlaPolicy { Priority = TicketPriority.Low, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 2880 },
            new SlaPolicy { Priority = TicketPriority.Medium, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440 },
            new SlaPolicy { Priority = TicketPriority.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 480 },
            new SlaPolicy { Priority = TicketPriority.Critical, ResponseTimeMinutes = 30, ResolutionTimeMinutes = 120 }
        };
        db.SlaPolicies.AddRange(slaPolicies);

        Department Dept(string name) => departments.First(d => d.Name == name);
        Category Cat(string name) => categories.First(c => c.Name == name);
        int SlaMinutes(TicketPriority p, bool resolution) =>
            resolution
                ? slaPolicies.First(s => s.Priority == p).ResolutionTimeMinutes
                : slaPolicies.First(s => s.Priority == p).ResponseTimeMinutes;

        var admin = new User
        {
            Name = "Alice Admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            Department = Dept("Management")
        };
        var agent1 = new User
        {
            Name = "Jane Doe",
            Email = "agent1@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Agent,
            Status = UserStatus.Active,
            Department = Dept("IT")
        };
        var agent2 = new User
        {
            Name = "Bob Support",
            Email = "agent2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Agent,
            Status = UserStatus.Active,
            Department = Dept("IT")
        };
        var employee = new User
        {
            Name = "John Smith",
            Email = "employee@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Employee,
            Status = UserStatus.Active,
            Department = Dept("Finance")
        };
        db.Users.AddRange(admin, agent1, agent2, employee);

        var now = DateTimeOffset.UtcNow;

        Ticket MakeTicket(
            string title, string description, TicketStatus status, TicketPriority priority,
            Category category, User createdBy, User? assignedAgent, string? location,
            DateTimeOffset createdAt, DateTimeOffset? resolvedAt = null, DateTimeOffset? closedAt = null)
        {
            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                Status = status,
                Priority = priority,
                Category = category,
                Department = createdBy.Department,
                CreatedBy = createdBy,
                AssignedAgent = assignedAgent,
                Location = location,
                CreatedAt = createdAt,
                ResolvedAt = resolvedAt,
                ClosedAt = closedAt,
                ResponseDueAt = createdAt.AddMinutes(SlaMinutes(priority, resolution: false)),
                ResolutionDueAt = createdAt.AddMinutes(SlaMinutes(priority, resolution: true))
            };

            ticket.Activities.Add(new TicketActivity
            {
                Ticket = ticket,
                User = createdBy,
                Type = TicketActivityType.Created,
                Description = $"Ticket created by {createdBy.Name}",
                CreatedAt = createdAt
            });

            if (assignedAgent is not null)
            {
                ticket.Activities.Add(new TicketActivity
                {
                    Ticket = ticket,
                    User = assignedAgent,
                    Type = TicketActivityType.Assigned,
                    Description = $"Ticket assigned to {assignedAgent.Name}",
                    CreatedAt = createdAt.AddMinutes(5)
                });
            }

            return ticket;
        }

        var vpnTicket = MakeTicket(
            "Cannot connect to VPN", "User cannot connect to the company VPN from home.",
            TicketStatus.InProgress, TicketPriority.High, Cat("Network"), employee, agent1,
            "Remote", now.AddHours(-3));
        vpnTicket.Comments.Add(new TicketComment { Ticket = vpnTicket, Author = agent1, Message = "Investigating the issue.", CreatedAt = now.AddHours(-2) });
        vpnTicket.Comments.Add(new TicketComment { Ticket = vpnTicket, Author = employee, Message = "It started this morning.", CreatedAt = now.AddHours(-1) });

        var printerTicket = MakeTicket(
            "Printer on 3rd floor not working", "Printer displays a paper jam error but no paper is jammed.",
            TicketStatus.Open, TicketPriority.Low, Cat("Printer"), employee, null,
            "Floor 3", now.AddHours(-6));

        var emailTicket = MakeTicket(
            "Cannot send emails with attachments", "Emails with attachments over 5MB fail to send.",
            TicketStatus.WaitingForUser, TicketPriority.Medium, Cat("Email"), employee, agent2,
            null, now.AddDays(-1));

        var breachTicket = MakeTicket(
            "Production database unreachable", "The production database is not responding to connections.",
            TicketStatus.Escalated, TicketPriority.Critical, Cat("Software"), employee, agent1,
            "Data Center", now.AddHours(-4));

        var resolvedAt = now.AddHours(-20);
        var laptopTicket = MakeTicket(
            "New laptop request", "Requesting a new laptop for a new hire starting next week.",
            TicketStatus.Resolved, TicketPriority.Medium, Cat("Hardware"), employee, agent2,
            null, now.AddDays(-2), resolvedAt: resolvedAt);
        laptopTicket.Activities.Add(new TicketActivity
        {
            Ticket = laptopTicket, User = agent2, Type = TicketActivityType.Resolved,
            Description = "Marked as resolved", CreatedAt = resolvedAt
        });

        var closedAt = now.AddDays(-5);
        var accessTicket = MakeTicket(
            "Account locked out", "User locked out of their account after failed login attempts.",
            TicketStatus.Closed, TicketPriority.High, Cat("Account Access"), employee, agent1,
            null, now.AddDays(-6), resolvedAt: now.AddDays(-6).AddHours(2), closedAt: closedAt);
        accessTicket.Activities.Add(new TicketActivity
        {
            Ticket = accessTicket, User = agent1, Type = TicketActivityType.Closed,
            Description = "Ticket closed", CreatedAt = closedAt
        });

        var securityTicket = MakeTicket(
            "Suspicious login attempt detected", "Multiple failed login attempts from an unrecognized location.",
            TicketStatus.Open, TicketPriority.Critical, Cat("Security"), employee, null,
            null, now.AddMinutes(-45));

        db.Tickets.AddRange(vpnTicket, printerTicket, emailTicket, breachTicket, laptopTicket, accessTicket, securityTicket);

        db.Notifications.AddRange(
            new Notification { User = agent1, Ticket = vpnTicket, Message = "You were assigned to ticket #\"Cannot connect to VPN\"", Type = NotificationType.TicketAssigned, CreatedAt = now.AddHours(-3) },
            new Notification { User = agent2, Ticket = emailTicket, Message = "You were assigned to ticket #\"Cannot send emails with attachments\"", Type = NotificationType.TicketAssigned, CreatedAt = now.AddDays(-1) },
            new Notification { User = agent1, Ticket = breachTicket, Message = "Ticket \"Production database unreachable\" was escalated", Type = NotificationType.TicketEscalated, CreatedAt = now.AddHours(-4) },
            new Notification { User = admin, Ticket = breachTicket, Message = "Ticket \"Production database unreachable\" was escalated", Type = NotificationType.TicketEscalated, IsRead = true, CreatedAt = now.AddHours(-4) },
            new Notification { User = employee, Ticket = laptopTicket, Message = "Your ticket \"New laptop request\" was resolved", Type = NotificationType.TicketResolved, IsRead = true, CreatedAt = resolvedAt }
        );

        await db.SaveChangesAsync();
    }
}

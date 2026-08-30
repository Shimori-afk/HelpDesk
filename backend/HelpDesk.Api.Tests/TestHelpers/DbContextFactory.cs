using HelpDesk.Api.Data;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Tests.TestHelpers;

public static class DbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task<ApplicationDbContext> CreateSeededDbAsync()
    {
        var db = CreateInMemoryDb();

        var category = new Category { Id = 1, Name = "Hardware" };
        db.Categories.Add(category);

        db.SlaPolicies.AddRange(
            new SlaPolicy { Id = 1, Priority = TicketPriority.Low, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 4320 },
            new SlaPolicy { Id = 2, Priority = TicketPriority.Medium, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440 },
            new SlaPolicy { Id = 3, Priority = TicketPriority.High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 480 },
            new SlaPolicy { Id = 4, Priority = TicketPriority.Critical, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 120 });

        db.Users.AddRange(
            new User { Id = 1, Name = "Ada Employee", Email = "employee@test.local", Role = UserRole.Employee, PasswordHash = "x" },
            new User { Id = 2, Name = "Grace Agent", Email = "agent@test.local", Role = UserRole.Agent, PasswordHash = "x" },
            new User { Id = 3, Name = "Alan Admin", Email = "admin@test.local", Role = UserRole.Admin, PasswordHash = "x" },
            new User { Id = 4, Name = "Other Employee", Email = "other@test.local", Role = UserRole.Employee, PasswordHash = "x" });

        await db.SaveChangesAsync();
        return db;
    }
}

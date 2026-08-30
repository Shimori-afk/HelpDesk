using HelpDesk.Api.Common;
using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class TicketService(ApplicationDbContext db, ICurrentUserService currentUser) : ITicketService
{
    public async Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters query)
    {
        var tickets = db.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedAgent)
            .AsQueryable();

        if (currentUser.Role == UserRole.Employee)
        {
            tickets = tickets.Where(t => t.CreatedById == currentUser.UserId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            tickets = tickets.Where(t =>
                EF.Functions.ILike(t.Title, $"%{search}%") ||
                EF.Functions.ILike(t.Description, $"%{search}%") ||
                EF.Functions.ILike(t.CreatedBy.Name, $"%{search}%"));
        }

        if (query.Status is not null) tickets = tickets.Where(t => t.Status == query.Status);
        if (query.Priority is not null) tickets = tickets.Where(t => t.Priority == query.Priority);
        if (query.CategoryId is not null) tickets = tickets.Where(t => t.CategoryId == query.CategoryId);
        if (query.DepartmentId is not null) tickets = tickets.Where(t => t.DepartmentId == query.DepartmentId);
        if (query.AssignedAgentId is not null) tickets = tickets.Where(t => t.AssignedAgentId == query.AssignedAgentId);
        if (query.Unassigned == true) tickets = tickets.Where(t => t.AssignedAgentId == null);
        if (query.CreatedFrom is not null) tickets = tickets.Where(t => t.CreatedAt >= query.CreatedFrom);
        if (query.CreatedTo is not null) tickets = tickets.Where(t => t.CreatedAt <= query.CreatedTo);

        tickets = (query.SortBy.ToLowerInvariant(), query.SortDescending) switch
        {
            ("priority", true) => tickets.OrderByDescending(t => t.Priority),
            ("priority", false) => tickets.OrderBy(t => t.Priority),
            ("status", true) => tickets.OrderByDescending(t => t.Status),
            ("status", false) => tickets.OrderBy(t => t.Status),
            ("title", true) => tickets.OrderByDescending(t => t.Title),
            ("title", false) => tickets.OrderBy(t => t.Title),
            (_, false) => tickets.OrderBy(t => t.CreatedAt),
            _ => tickets.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await tickets.CountAsync();

        var items = await tickets
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TicketListItemDto(
                t.Id, t.Title, t.Status, t.Priority,
                t.Category.Name, t.Department != null ? t.Department.Name : null,
                t.CreatedById, t.CreatedBy.Name,
                t.AssignedAgentId, t.AssignedAgent != null ? t.AssignedAgent.Name : null,
                t.CreatedAt, t.ResponseDueAt, t.ResolutionDueAt, t.ResolvedAt))
            .ToListAsync();

        return new PagedResult<TicketListItemDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<TicketDetailDto> GetTicketByIdAsync(int id)
    {
        var ticket = await LoadTicketForDetailAsync(id);
        EnsureCanView(ticket);
        return ToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> CreateTicketAsync(CreateTicketDto dto)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
        {
            throw new BadRequestAppException("Invalid category.");
        }

        if (dto.DepartmentId is not null && !await db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
        {
            throw new BadRequestAppException("Invalid department.");
        }

        var slaPolicy = await db.SlaPolicies.FirstOrDefaultAsync(s => s.Priority == dto.Priority)
            ?? throw new BadRequestAppException("No SLA policy configured for this priority.");

        var now = DateTimeOffset.UtcNow;
        var ticket = new Ticket
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Status = TicketStatus.Open,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            DepartmentId = dto.DepartmentId,
            Location = dto.Location,
            CreatedById = currentUser.UserId,
            CreatedAt = now,
            ResponseDueAt = now.AddMinutes(slaPolicy.ResponseTimeMinutes),
            ResolutionDueAt = now.AddMinutes(slaPolicy.ResolutionTimeMinutes)
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        db.TicketActivities.Add(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = currentUser.UserId,
            Type = TicketActivityType.Created,
            Description = "Ticket created",
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var loaded = await LoadTicketForDetailAsync(ticket.Id);
        return ToDetailDto(loaded);
    }

    public async Task<TicketDetailDto> UpdateTicketAsync(int id, UpdateTicketDto dto)
    {
        var ticket = await LoadTicketForDetailAsync(id);
        EnsureCanView(ticket);
        EnsureCanEdit(ticket);

        if (!await db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
        {
            throw new BadRequestAppException("Invalid category.");
        }

        if (dto.DepartmentId is not null && !await db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
        {
            throw new BadRequestAppException("Invalid department.");
        }

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description.Trim();
        ticket.CategoryId = dto.CategoryId;
        ticket.DepartmentId = dto.DepartmentId;
        ticket.Location = dto.Location;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();

        var reloaded = await LoadTicketForDetailAsync(id);
        return ToDetailDto(reloaded);
    }

    public async Task DeleteTicketAsync(int id)
    {
        var ticket = await db.Tickets.FindAsync(id) ?? throw new NotFoundException("Ticket not found.");
        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TicketActivityDto>> GetActivitiesAsync(int ticketId)
    {
        var ticket = await LoadTicketForDetailAsync(ticketId);
        EnsureCanView(ticket);
        return ticket.Activities
            .OrderBy(a => a.CreatedAt)
            .Select(ToActivityDto)
            .ToList();
    }

    public async Task<TicketCommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto)
    {
        var ticket = await LoadTicketForDetailAsync(ticketId);
        EnsureCanView(ticket);

        var now = DateTimeOffset.UtcNow;
        var comment = new TicketComment
        {
            TicketId = ticketId,
            AuthorId = currentUser.UserId,
            Message = dto.Message.Trim(),
            CreatedAt = now
        };
        db.TicketComments.Add(comment);

        db.TicketActivities.Add(new TicketActivity
        {
            TicketId = ticketId,
            UserId = currentUser.UserId,
            Type = TicketActivityType.CommentAdded,
            Description = "Comment added",
            CreatedAt = now
        });

        var recipientIds = new HashSet<int> { ticket.CreatedById };
        if (ticket.AssignedAgentId is not null) recipientIds.Add(ticket.AssignedAgentId.Value);
        recipientIds.Remove(currentUser.UserId);

        foreach (var recipientId in recipientIds)
        {
            db.Notifications.Add(new Notification
            {
                UserId = recipientId,
                TicketId = ticketId,
                Message = $"New comment on ticket \"{ticket.Title}\"",
                Type = NotificationType.CommentAdded,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync();

        var author = await db.Users.FindAsync(currentUser.UserId);
        return new TicketCommentDto(comment.Id, comment.AuthorId, author?.Name ?? "Unknown", comment.Message, comment.CreatedAt);
    }

    private async Task<Ticket> LoadTicketForDetailAsync(int id)
    {
        return await db.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Include(t => t.Activities).ThenInclude(a => a.User)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException("Ticket not found.");
    }

    private void EnsureCanView(Ticket ticket)
    {
        if (currentUser.Role == UserRole.Employee && ticket.CreatedById != currentUser.UserId)
        {
            throw new NotFoundException("Ticket not found.");
        }
    }

    private void EnsureCanEdit(Ticket ticket)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return;
        }

        var isOwner = ticket.CreatedById == currentUser.UserId;
        var isEditableStatus = ticket.Status is TicketStatus.Open or TicketStatus.WaitingForUser;

        if (!isOwner || !isEditableStatus)
        {
            throw new ForbiddenAppException("This ticket can no longer be edited.");
        }
    }

    private static TicketDetailDto ToDetailDto(Ticket t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.Location,
        t.CategoryId, t.Category.Name, t.DepartmentId, t.Department?.Name,
        t.CreatedById, t.CreatedBy.Name, t.AssignedAgentId, t.AssignedAgent?.Name,
        t.CreatedAt, t.UpdatedAt, t.ResolvedAt, t.ClosedAt, t.ResponseDueAt, t.ResolutionDueAt, t.RespondedAt,
        t.Comments.OrderBy(c => c.CreatedAt).Select(c => new TicketCommentDto(c.Id, c.AuthorId, c.Author.Name, c.Message, c.CreatedAt)).ToList(),
        t.Activities.OrderBy(a => a.CreatedAt).Select(ToActivityDto).ToList(),
        t.Attachments.Select(a => new AttachmentDto(a.Id, a.FileName, a.ContentType, a.SizeBytes, a.CreatedAt)).ToList());

    private static TicketActivityDto ToActivityDto(TicketActivity a) =>
        new(a.Id, a.UserId, a.User.Name, a.Type, a.Description, a.OldValue, a.NewValue, a.CreatedAt);
}

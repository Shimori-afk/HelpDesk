using HelpDesk.Api.Common;
using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(ApplicationDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll()
    {
        var notifications = await db.Notifications
            .Where(n => n.UserId == currentUser.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.TicketId, n.Message, n.Type, n.IsRead, n.CreatedAt))
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new NotFoundException("Notification not found.");

        if (notification.UserId != currentUser.UserId)
        {
            throw new NotFoundException("Notification not found.");
        }

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await db.Notifications
            .Where(n => n.UserId == currentUser.UserId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

        return NoContent();
    }
}

using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Agent,Admin")]
[Route("api/users")]
public class UsersController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll([FromQuery] UserRole? role)
    {
        var query = db.Users.Include(u => u.Department).AsQueryable();

        if (role is not null)
        {
            query = query.Where(u => u.Role == role);
        }

        var users = await query
            .OrderBy(u => u.Name)
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role, u.Status, u.DepartmentId, u.Department != null ? u.Department.Name : null, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }
}

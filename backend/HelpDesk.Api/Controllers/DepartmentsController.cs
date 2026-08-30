using FluentValidation;
using HelpDesk.Api.Common;
using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departments")]
public class DepartmentsController(ApplicationDbContext db, IValidator<UpsertDepartmentDto> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll()
    {
        var departments = await db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name))
            .ToListAsync();
        return Ok(departments);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(UpsertDepartmentDto dto)
    {
        await validator.ValidateAndThrowAppExceptionAsync(dto);

        if (await db.Departments.AnyAsync(d => d.Name == dto.Name))
        {
            throw new ConflictException("A department with this name already exists.");
        }

        var department = new Department { Name = dto.Name.Trim() };
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new DepartmentDto(department.Id, department.Name));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, UpsertDepartmentDto dto)
    {
        await validator.ValidateAndThrowAppExceptionAsync(dto);

        var department = await db.Departments.FindAsync(id) ?? throw new NotFoundException("Department not found.");
        department.Name = dto.Name.Trim();
        await db.SaveChangesAsync();
        return Ok(new DepartmentDto(department.Id, department.Name));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await db.Departments.FindAsync(id) ?? throw new NotFoundException("Department not found.");

        if (await db.Tickets.AnyAsync(t => t.DepartmentId == id) || await db.Users.AnyAsync(u => u.DepartmentId == id))
        {
            throw new ConflictException("Cannot delete a department that is still in use.");
        }

        db.Departments.Remove(department);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

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
[Route("api/categories")]
public class CategoriesController(ApplicationDbContext db, IValidator<UpsertCategoryDto> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll()
    {
        var categories = await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();
        return Ok(categories);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(UpsertCategoryDto dto)
    {
        await validator.ValidateAndThrowAppExceptionAsync(dto);

        if (await db.Categories.AnyAsync(c => c.Name == dto.Name))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var category = new Category { Name = dto.Name.Trim() };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new CategoryDto(category.Id, category.Name));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpsertCategoryDto dto)
    {
        await validator.ValidateAndThrowAppExceptionAsync(dto);

        var category = await db.Categories.FindAsync(id) ?? throw new NotFoundException("Category not found.");
        category.Name = dto.Name.Trim();
        await db.SaveChangesAsync();
        return Ok(new CategoryDto(category.Id, category.Name));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id) ?? throw new NotFoundException("Category not found.");

        if (await db.Tickets.AnyAsync(t => t.CategoryId == id))
        {
            throw new ConflictException("Cannot delete a category that has tickets assigned to it.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

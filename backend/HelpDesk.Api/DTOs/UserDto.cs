using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs;

public record UserDto(
    int Id,
    string Name,
    string Email,
    UserRole Role,
    UserStatus Status,
    int? DepartmentId,
    string? DepartmentName,
    DateTimeOffset CreatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.Role,
        user.Status,
        user.DepartmentId,
        user.Department?.Name,
        user.CreatedAt);
}

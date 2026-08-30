namespace HelpDesk.Api.DTOs.Auth;

public record RegisterRequestDto(string Name, string Email, string Password, int? DepartmentId);

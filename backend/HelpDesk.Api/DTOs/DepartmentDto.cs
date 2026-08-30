namespace HelpDesk.Api.DTOs;

public record DepartmentDto(int Id, string Name);

public record UpsertDepartmentDto(string Name);

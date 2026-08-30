namespace HelpDesk.Api.DTOs;

public record CategoryDto(int Id, string Name);

public record UpsertCategoryDto(string Name);

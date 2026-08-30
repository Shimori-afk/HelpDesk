namespace HelpDesk.Api.DTOs.Tickets;

public record AttachmentDto(int Id, string FileName, string ContentType, long SizeBytes, DateTimeOffset CreatedAt);

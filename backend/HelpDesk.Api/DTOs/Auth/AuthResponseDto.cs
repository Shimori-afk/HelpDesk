namespace HelpDesk.Api.DTOs.Auth;

public record AuthResponseDto(string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, UserDto User);

using HelpDesk.Api.Models;

namespace HelpDesk.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

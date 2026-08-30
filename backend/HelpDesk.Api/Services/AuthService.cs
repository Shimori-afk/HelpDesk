using HelpDesk.Api.Common;
using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.DTOs.Auth;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDesk.Api.Services;

public class AuthService(ApplicationDbContext db, IJwtTokenService tokenService, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        if (dto.DepartmentId is not null && !await db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
        {
            throw new BadRequestAppException("Invalid department.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Employee,
            Status = UserStatus.Active,
            DepartmentId = dto.DepartmentId
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Department).LoadAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenAppException("This account has been deactivated.");
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var existing = await db.RefreshTokens
            .Include(r => r.User).ThenInclude(u => u.Department)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (existing is null || !existing.IsActive)
        {
            throw new UnauthorizedAppException("Invalid or expired refresh token.");
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;

        return await IssueTokensAsync(existing.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (existing is not null && existing.RevokedAt is null)
        {
            existing.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        });

        await db.SaveChangesAsync();

        return new AuthResponseDto(accessToken, expiresAt, refreshToken, UserDto.FromEntity(user));
    }
}

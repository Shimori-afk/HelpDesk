using System.Security.Claims;
using HelpDesk.Api.Common.Exceptions;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Common;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : throw new UnauthorizedAppException("Not authenticated.");
        }
    }

    public UserRole Role
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(value, out var role) ? role : throw new UnauthorizedAppException("Not authenticated.");
        }
    }
}

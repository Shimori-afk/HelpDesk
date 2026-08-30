using HelpDesk.Api.Models;

namespace HelpDesk.Api.Common;

public interface ICurrentUserService
{
    int UserId { get; }
    UserRole Role { get; }
}

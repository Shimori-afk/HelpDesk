using HelpDesk.Api.Common;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Tests.TestHelpers;

public class FakeCurrentUserService(int userId, UserRole role) : ICurrentUserService
{
    public int UserId { get; set; } = userId;
    public UserRole Role { get; set; } = role;
}

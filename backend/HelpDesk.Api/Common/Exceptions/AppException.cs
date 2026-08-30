using System.Net;

namespace HelpDesk.Api.Common.Exceptions;

public abstract class AppException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : AppException(HttpStatusCode.NotFound, message);

public class ConflictException(string message) : AppException(HttpStatusCode.Conflict, message);

public class UnauthorizedAppException(string message) : AppException(HttpStatusCode.Unauthorized, message);

public class ForbiddenAppException(string message) : AppException(HttpStatusCode.Forbidden, message);

public class BadRequestAppException(string message) : AppException(HttpStatusCode.BadRequest, message);

public class ValidationAppException(IDictionary<string, string[]> errors)
    : AppException(HttpStatusCode.UnprocessableEntity, "One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

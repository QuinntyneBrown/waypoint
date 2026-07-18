namespace WordUp.Application.Common;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have access to this resource.") : base(403, message) { }
}

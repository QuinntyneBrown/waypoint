namespace WordUp.Application.Shared.Common;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message = "Not found.") : base(404, message) { }
}

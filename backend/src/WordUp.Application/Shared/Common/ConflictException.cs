namespace WordUp.Application.Shared.Common;

public sealed class ConflictException : AppException
{
    public ConflictException(string message = "The request conflicts with the current state.") : base(409, message) { }
}

namespace WordUp.Application.Common;

/// <summary>Base for expected application errors that map to a specific HTTP status.</summary>
public abstract class AppException : Exception
{
    protected AppException(int statusCode, string message) : base(message) => StatusCode = statusCode;

    public int StatusCode { get; }
}

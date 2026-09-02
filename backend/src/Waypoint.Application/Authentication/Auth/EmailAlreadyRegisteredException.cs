namespace Waypoint.Application.Authentication.Auth;

public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email)
        : base($"Email '{email}' is already registered.") { }
}

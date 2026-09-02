namespace Waypoint.Application.Authentication.Auth;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.") { }
}

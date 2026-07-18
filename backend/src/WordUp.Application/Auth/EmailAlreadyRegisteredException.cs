namespace WordUp.Application.Auth;

public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email)
        : base($"Email '{email}' is already registered.") { }
}

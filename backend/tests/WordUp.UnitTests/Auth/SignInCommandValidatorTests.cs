using WordUp.Application.Auth;

namespace WordUp.UnitTests.Auth;

// Traces to: L2-AUTH-001 (email/password sign-in — input validation)
[TestFixture]
[Property("Req", "L2-AUTH-001")]
public sealed class SignInCommandValidatorTests
{
    private readonly SignInCommandValidator _validator = new();

    [Test]
    public void Valid_email_and_password_pass_validation()
    {
        var result = _validator.Validate(new SignInCommand("jayden@example.com", "Passw0rd!"));
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", "Passw0rd!", Description = "empty email")]
    [TestCase("not-an-email", "Passw0rd!", Description = "malformed email")]
    [TestCase("jayden@example.com", "", Description = "empty password")]
    public void Missing_or_malformed_input_fails_validation(string email, string password)
    {
        var result = _validator.Validate(new SignInCommand(email, password));
        Assert.That(result.IsValid, Is.False);
    }
}

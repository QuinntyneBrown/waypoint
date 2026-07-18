namespace WordUp.Api.Controllers;

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

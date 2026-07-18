namespace WordUp.Api.Contracts.Authentication;

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

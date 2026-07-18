namespace WordUp.Api.Contracts.Engagement;

public record SubmitRsvpRequest(string Response, string? Note, bool ReminderSet);

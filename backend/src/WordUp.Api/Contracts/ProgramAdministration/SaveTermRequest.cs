namespace WordUp.Api.Contracts.ProgramAdministration;

public sealed record SaveTermRequest(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status);

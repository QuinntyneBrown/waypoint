namespace WordUp.Api.Contracts.Engagement;

public record SaveAttendanceRequest(Guid SessionId, List<AttendanceMarkInput>? Marks);

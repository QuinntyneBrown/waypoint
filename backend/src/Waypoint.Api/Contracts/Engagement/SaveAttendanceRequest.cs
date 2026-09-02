namespace Waypoint.Api.Contracts.Engagement;

public record SaveAttendanceRequest(Guid SessionId, List<AttendanceMarkInput>? Marks);

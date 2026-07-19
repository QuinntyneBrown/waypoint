namespace WordUp.Api.Authorization;

/// <summary>
/// Named authorization policies mapping to the L2-AUTH-009 permission matrix. Role checks are enforced
/// here; resource-scoped rules (e.g. a mentor only their assigned youth) are enforced in handlers.
/// </summary>
public static class Policies
{
    public const string YouthParticipation = "YouthParticipation";
    public const string ManageUsers = "ManageUsers";
    public const string ApproveApplications = "ApproveApplications";
    public const string EditCurriculum = "EditCurriculum";
    public const string InviteUsers = "InviteUsers";
    public const string RecordAttendance = "RecordAttendance";
    public const string ViewAuditLog = "ViewAuditLog";
    public const string ImportExportData = "ImportExportData";
}

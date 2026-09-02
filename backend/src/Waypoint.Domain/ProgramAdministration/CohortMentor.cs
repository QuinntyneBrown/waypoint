namespace Waypoint.Domain.ProgramAdministration;

/// <summary>Associates a mentor with a cohort as lead or supporting; a mentor can hold different
/// roles across cohorts (L2-ADMIN-005 AC2).</summary>
public class CohortMentor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CohortId { get; set; }
    public Guid MentorId { get; set; }
    public bool IsLead { get; set; }
}

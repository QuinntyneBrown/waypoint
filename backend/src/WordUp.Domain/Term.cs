namespace WordUp.Domain;

/// <summary>A program term / cohort cycle (L2-ADMIN-006 AC2).</summary>
public class Term
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Upcoming"; // Upcoming | Active | Completed
}

namespace BusTracking.Common.Entities;

public class AcademicYear : IMultiTenant
{
    public int AcademicYearId { get; set; }
    public int? SchoolId { get; set; }
    public string YearName { get; set; } = string.Empty; // e.g., "2025-2026"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCurrent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual School? School { get; set; }
}

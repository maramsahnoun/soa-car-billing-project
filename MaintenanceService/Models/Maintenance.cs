namespace MaintenanceService.Models;

/// <summary>
/// Represents a maintenance event for a vehicle.
/// Supports preventive, corrective, and accident-related maintenance.
/// </summary>
public class Maintenance
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    /// <summary>
    /// Type of maintenance: PREVENTIVE, CORRECTIVE, ACCIDENT
    /// </summary>
    public string MaintenanceType { get; set; } = "PREVENTIVE";

    /// <summary>
    /// Scheduled date for maintenance
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Status: SCHEDULED, IN_PROGRESS, COMPLETED, CANCELLED
    /// </summary>
    public string Status { get; set; } = "SCHEDULED";

    /// <summary>
    /// Estimated cost in TND
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Actual cost after completion
    /// </summary>
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Vehicle mileage at time of maintenance
    /// </summary>
    public int CurrentMileage { get; set; }

    /// <summary>
    /// Detailed description of work needed
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Mechanic's notes and observations
    /// </summary>
    public string MechanicNotes { get; set; } = "";

    /// <summary>
    /// Priority level: LOW, MEDIUM, HIGH, URGENT
    /// </summary>
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>
    /// Collection of spare parts used
    /// </summary>
    public ICollection<SparePart> SpareParts { get; set; } = new List<SparePart>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

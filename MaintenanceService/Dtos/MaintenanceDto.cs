using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// General-purpose DTO for Maintenance entity (backward compatible)
/// </summary>
[DataContract]
public class MaintenanceDto
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public int VehicleId { get; set; }

    [DataMember]
    public string MaintenanceType { get; set; } = "PREVENTIVE";

    [DataMember]
    public DateTime ScheduledDate { get; set; }

    [DataMember]
    public DateTime? CompletedDate { get; set; }

    [DataMember]
    public string Status { get; set; } = "SCHEDULED";

    [DataMember]
    public decimal EstimatedCost { get; set; }

    [DataMember]
    public decimal? ActualCost { get; set; }

    [DataMember]
    public int CurrentMileage { get; set; }

    [DataMember]
    public string Description { get; set; } = "";

    [DataMember]
    public string MechanicNotes { get; set; } = "";

    [DataMember]
    public string Priority { get; set; } = "MEDIUM";

    [DataMember]
    public List<SparePartDto> SpareParts { get; set; } = new();

    [DataMember]
    public DateTime CreatedAt { get; set; }

    [DataMember]
    public DateTime? UpdatedAt { get; set; }
}

using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// Response DTO for maintenance operations
/// </summary>
[DataContract]
public class MaintenanceResponse
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public int VehicleId { get; set; }

    [DataMember]
    public string MaintenanceType { get; set; } = "";

    [DataMember]
    public DateTime ScheduledDate { get; set; }

    [DataMember]
    public DateTime? CompletedDate { get; set; }

    [DataMember]
    public string Status { get; set; } = "";

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
    public string Priority { get; set; } = "";

    [DataMember]
    public List<SparePartDto> SpareParts { get; set; } = new();

    [DataMember]
    public DateTime CreatedAt { get; set; }

    [DataMember]
    public DateTime? UpdatedAt { get; set; }
}

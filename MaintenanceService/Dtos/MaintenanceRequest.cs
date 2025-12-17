using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// Request DTO for scheduling a new maintenance
/// </summary>
[DataContract]
public class MaintenanceRequest
{
    [DataMember]
    public int VehicleId { get; set; }

    [DataMember]
    public string MaintenanceType { get; set; } = "PREVENTIVE"; // PREVENTIVE, CORRECTIVE, ACCIDENT

    [DataMember]
    public DateTime ScheduledDate { get; set; }

    [DataMember]
    public string Description { get; set; } = "";

    [DataMember]
    public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, URGENT

    [DataMember]
    public decimal EstimatedCost { get; set; }

    [DataMember]
    public int CurrentMileage { get; set; }
}

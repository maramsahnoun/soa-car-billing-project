using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// DTO for vehicle condition report
/// </summary>
[DataContract]
public class ConditionReport
{
    [DataMember]
    public int VehicleId { get; set; }

    [DataMember]
    public int CurrentMileage { get; set; }

    [DataMember]
    public string GeneralCondition { get; set; } = ""; // EXCELLENT, GOOD, FAIR, POOR

    [DataMember]
    public string Notes { get; set; } = "";

    [DataMember]
    public List<string> IssuesFound { get; set; } = new();

    [DataMember]
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
}

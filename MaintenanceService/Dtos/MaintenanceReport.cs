using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// DTO for maintenance report
/// </summary>
[DataContract]
public class MaintenanceReport
{
    [DataMember]
    public DateTime GeneratedDate { get; set; }

    [DataMember]
    public DateTime StartDate { get; set; }

    [DataMember]
    public DateTime EndDate { get; set; }

    [DataMember]
    public int TotalMaintenances { get; set; }

    [DataMember]
    public int CompletedMaintenances { get; set; }

    [DataMember]
    public int PendingMaintenances { get; set; }

    [DataMember]
    public decimal TotalCost { get; set; }

    [DataMember]
    public List<MaintenanceReportEntry> Entries { get; set; } = new();
}

/// <summary>
/// Entry in maintenance report
/// </summary>
[DataContract]
public class MaintenanceReportEntry
{
    [DataMember]
    public int MaintenanceId { get; set; }

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
    public decimal ActualCost { get; set; }

    [DataMember]
    public string Priority { get; set; } = "";
}

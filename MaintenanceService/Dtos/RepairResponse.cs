using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// Response DTO for repair operations
/// </summary>
[DataContract]
public class RepairResponse
{
    [DataMember]
    public int MaintenanceId { get; set; }

    [DataMember]
    public string Status { get; set; } = "";

    [DataMember]
    public decimal TotalCost { get; set; }

    [DataMember]
    public DateTime CompletedDate { get; set; }

    [DataMember]
    public List<SparePartDto> SpareParts { get; set; } = new();
}

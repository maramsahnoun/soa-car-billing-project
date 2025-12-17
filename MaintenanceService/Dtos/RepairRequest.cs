using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// Request DTO for recording a repair/maintenance work
/// </summary>
[DataContract]
public class RepairRequest
{
    [DataMember]
    public int MaintenanceId { get; set; }

    [DataMember]
    public string MechanicNotes { get; set; } = "";

    [DataMember]
    public decimal ActualCost { get; set; }

    [DataMember]
    public List<SparePartRequest> SpareParts { get; set; } = new();
}

/// <summary>
/// Request DTO for spare parts in repair
/// </summary>
[DataContract]
public class SparePartRequest
{
    [DataMember]
    public string Name { get; set; } = "";

    [DataMember]
    public string PartNumber { get; set; } = "";

    [DataMember]
    public int Quantity { get; set; }

    [DataMember]
    public decimal UnitPrice { get; set; }
}

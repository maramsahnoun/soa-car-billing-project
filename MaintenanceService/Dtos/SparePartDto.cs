using System.Runtime.Serialization;

namespace MaintenanceService.Dtos;

/// <summary>
/// DTO for spare parts
/// </summary>
[DataContract]
public class SparePartDto
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public int MaintenanceId { get; set; }

    [DataMember]
    public string Name { get; set; } = "";

    [DataMember]
    public string PartNumber { get; set; } = "";

    [DataMember]
    public int Quantity { get; set; }

    [DataMember]
    public decimal UnitPrice { get; set; }

    [DataMember]
    public decimal TotalPrice { get; set; }

    [DataMember]
    public DateTime CreatedAt { get; set; }
}

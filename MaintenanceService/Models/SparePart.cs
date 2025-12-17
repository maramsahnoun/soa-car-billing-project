namespace MaintenanceService.Models;

/// <summary>
/// Represents a spare part used in a maintenance operation.
/// </summary>
public class SparePart
{
    public int Id { get; set; }

    public int MaintenanceId { get; set; }

    /// <summary>
    /// Name of the spare part
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Part number/SKU for inventory tracking
    /// </summary>
    public string PartNumber { get; set; } = "";

    /// <summary>
    /// Quantity used
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price in TND
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price: Quantity * UnitPrice
    /// </summary>
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property
    /// </summary>
    public Maintenance? Maintenance { get; set; }
}

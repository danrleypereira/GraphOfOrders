using System.Text.Json;

namespace DeliveryService.Domain;

public class DeliveryOrder
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Available"; // Available, Assigned, InTransit, Delivered, Failed
    public int? DeliveryPersonId { get; set; }
    public string? SnapshotJson { get; set; } // JSONB field for order details
    public string? IdempotencyToken { get; set; }

    // Navigation property
    public virtual DeliveryPerson? DeliveryPerson { get; set; }

    // Computed property for snapshot
    public Dictionary<string, object>? Snapshot
    {
        get => string.IsNullOrEmpty(SnapshotJson)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(SnapshotJson);
        set => SnapshotJson = value == null
            ? null
            : JsonSerializer.Serialize(value);
    }
}
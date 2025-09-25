namespace DeliveryService.Domain;

public class DeliveryPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();
}
using System;
using System.Collections.Generic;

namespace GraphOfOrders.Lib.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int BrandId { get; set; }  // Foreign key
        public int CustomerId { get; set; }  // Foreign key
        public DateTime OrderDate { get; set; }
        
        // Delivery fields moved to delivery-service
        // public int? DeliverPersonId { get; set; }
        // public DateTime? DeliveryDate { get; set; }
        // public string DeliveryStatus { get; set; } = "Pending";

        // Navigation properties
        public virtual Brand Brand { get; set; }
        public virtual Customer Customer { get; set; }
        // DeliverPerson navigation moved to delivery-service
    }
}
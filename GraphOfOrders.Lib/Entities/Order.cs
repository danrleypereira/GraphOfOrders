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
        
        // Delivery fields
        public int? DeliverPersonId { get; set; }  // Foreign key (nullable - pode não ter entregador ainda)
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; } = "Pending";  // "Pending", "InTransit", "Delivered"

        // Navigation properties
        public virtual Brand Brand { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual DeliverPerson DeliverPerson { get; set; }
    }
}
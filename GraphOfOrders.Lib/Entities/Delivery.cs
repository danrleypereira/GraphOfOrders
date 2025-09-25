using System;

namespace GraphOfOrders.Lib.Entities
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int OrderId { get; set; }  // Foreign key
        public int DeliverPersonId { get; set; }  // Foreign key
        public DateTime DeliveryDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryStatus { get; set; }  // "Pending", "InTransit", "Delivered", "Failed"
        public string Notes { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual DeliverPerson DeliverPerson { get; set; }
    }
}

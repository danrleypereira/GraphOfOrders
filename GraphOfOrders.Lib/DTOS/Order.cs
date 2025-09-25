using System;

namespace GraphOfOrders.Lib.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int BrandId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        
        // Delivery fields
        public int? DeliverPersonId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
        
        // Navigation info
        public string DeliverPersonName { get; set; }
    }
}
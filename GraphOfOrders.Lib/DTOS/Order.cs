using System;

namespace GraphOfOrders.Lib.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int BrandId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        
        // Delivery fields moved to delivery-service
    }
}
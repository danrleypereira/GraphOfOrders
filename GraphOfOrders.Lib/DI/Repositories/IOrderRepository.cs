using GraphOfOrders.Lib.Entities;
using System;
using System.Collections.Generic;

namespace GraphOfOrders.Lib.DI
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrdersByBrand(int brandId);
        // Delivery-related methods moved to delivery-service
    }

}

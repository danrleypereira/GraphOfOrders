using GraphOfOrders.Lib.Entities;
using System;
using System.Collections.Generic;

namespace GraphOfOrders.Lib.DI
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrdersByBrand(int brandId);
        IEnumerable<Order> GetOrdersByDeliverPerson(int deliverPersonId);
        IEnumerable<Order> GetOrdersByDeliveryStatus(string status);
        void AssignDeliverPerson(int orderId, int deliverPersonId);
        void UpdateDeliveryStatus(int orderId, string status, DateTime? deliveryDate = null);
    }

}

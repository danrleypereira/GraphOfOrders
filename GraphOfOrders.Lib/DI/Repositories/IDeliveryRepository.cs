using GraphOfOrders.Lib.Entities;
using System.Collections.Generic;

namespace GraphOfOrders.Lib.DI
{
    public interface IDeliveryRepository
    {
        IEnumerable<Delivery> GetAllDeliveries();
        Delivery GetDeliveryById(int deliveryId);
        IEnumerable<Delivery> GetDeliveriesByOrder(int orderId);
        IEnumerable<Delivery> GetDeliveriesByDeliverPerson(int deliverPersonId);
        IEnumerable<Delivery> GetDeliveriesByStatus(string status);
        void AddDelivery(Delivery delivery);
        void UpdateDelivery(Delivery delivery);
        void DeleteDelivery(int deliveryId);
    }
}

using GraphOfOrders.Lib.Entities;
using GraphOfOrders.Lib.DI;
using Microsoft.EntityFrameworkCore;

namespace GraphOfOrders.Repo {
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersContext _context;

        public OrderRepository(OrdersContext context)
        {
            _context = context;
        }

        public IEnumerable<Order> GetOrdersByBrand(int brandId)
        {
            return _context.Orders
                .Include(o => o.DeliverPerson)
                .Where(o => o.BrandId == brandId)
                .ToList();
        }

        public IEnumerable<Order> GetOrdersByDeliverPerson(int deliverPersonId)
        {
            return _context.Orders
                .Include(o => o.DeliverPerson)
                .Include(o => o.Customer)
                .Include(o => o.Brand)
                .Where(o => o.DeliverPersonId == deliverPersonId)
                .ToList();
        }

        public IEnumerable<Order> GetOrdersByDeliveryStatus(string status)
        {
            return _context.Orders
                .Include(o => o.DeliverPerson)
                .Include(o => o.Customer)
                .Include(o => o.Brand)
                .Where(o => o.DeliveryStatus == status)
                .ToList();
        }

        public void AssignDeliverPerson(int orderId, int deliverPersonId)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.DeliverPersonId = deliverPersonId;
                order.DeliveryStatus = "Assigned";
                _context.SaveChanges();
            }
        }

        public void UpdateDeliveryStatus(int orderId, string status, DateTime? deliveryDate = null)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.DeliveryStatus = status;
                if (deliveryDate.HasValue)
                {
                    order.DeliveryDate = deliveryDate.Value;
                }
                _context.SaveChanges();
            }
        }
    }
}

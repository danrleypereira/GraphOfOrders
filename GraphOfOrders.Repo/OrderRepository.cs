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
                .Include(o => o.Customer)
                .Include(o => o.Brand)
                .Where(o => o.BrandId == brandId)
                .ToList();
        }

        // Delivery-related methods moved to delivery-service
    }
}

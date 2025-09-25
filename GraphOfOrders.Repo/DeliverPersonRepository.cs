using GraphOfOrders.Lib.Entities;
using GraphOfOrders.Lib.DI;
using Microsoft.EntityFrameworkCore;

namespace GraphOfOrders.Repo
{
    public class DeliverPersonRepository : IDeliverPersonRepository
    {
        private readonly OrdersContext _context;

        public DeliverPersonRepository(OrdersContext context)
        {
            _context = context;
        }

        public IEnumerable<DeliverPerson> GetAllDeliverPersons()
        {
            return _context.DeliverPersons
                .Include(dp => dp.Orders)
                .ToList();
        }

        public DeliverPerson GetDeliverPersonById(int deliverPersonId)
        {
            return _context.DeliverPersons
                .Include(dp => dp.Orders)
                .FirstOrDefault(dp => dp.DeliverPersonId == deliverPersonId);
        }

        public IEnumerable<DeliverPerson> GetActiveDeliverPersons()
        {
            return _context.DeliverPersons
                .Where(dp => dp.IsActive)
                .ToList();
        }

        public void AddDeliverPerson(DeliverPerson deliverPerson)
        {
            _context.DeliverPersons.Add(deliverPerson);
            _context.SaveChanges();
        }

        public void UpdateDeliverPerson(DeliverPerson deliverPerson)
        {
            _context.DeliverPersons.Update(deliverPerson);
            _context.SaveChanges();
        }
    }
}

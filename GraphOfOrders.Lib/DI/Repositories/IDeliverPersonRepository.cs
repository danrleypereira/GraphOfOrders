using GraphOfOrders.Lib.Entities;
using System.Collections.Generic;

namespace GraphOfOrders.Lib.DI
{
    public interface IDeliverPersonRepository
    {
        IEnumerable<DeliverPerson> GetAllDeliverPersons();
        DeliverPerson GetDeliverPersonById(int deliverPersonId);
        IEnumerable<DeliverPerson> GetActiveDeliverPersons();
        void AddDeliverPerson(DeliverPerson deliverPerson);
        void UpdateDeliverPerson(DeliverPerson deliverPerson);
    }
}

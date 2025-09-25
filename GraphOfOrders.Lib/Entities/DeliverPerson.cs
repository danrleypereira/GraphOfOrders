using System.Collections.Generic;

namespace GraphOfOrders.Lib.Entities
{
    public class DeliverPerson
    {
        public int DeliverPersonId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; }
    }
}
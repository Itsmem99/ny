using Pharmacy.Models;
using System;
using System.Collections.Generic;

namespace Pharmacy.Models
{

    namespace Pharmacy.Models
    {
        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public List<OrderItem> OrderItems { get; set; } = new();

            public DateTime OrderDate { get; set; } = DateTime.Now;
        }

        public class OrderItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }

            public virtual Order Order { get; set; }
            public virtual Product Product { get; set; }
        }
    }
}

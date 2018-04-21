using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperTesting.Models
{
    [Table("[Order Details]")]
    public class OrderDetail
    {        
        [ExplicitKey]
        public int OrderId { get; set; }

        [ExplicitKey]
        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Discount { get; set; }

    }
}

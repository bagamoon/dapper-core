using System;
using System.Collections.Generic;

namespace Benchmark.Models
{
    public partial class OrderDetails
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }

        public Orders Order { get; set; }
        public Products Product { get; set; }
    }
}

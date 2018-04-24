using System;
using System.Collections.Generic;

namespace Benchmark.Models
{
    public partial class Bets
    {
        public long BetId { get; set; }
        public long MemberId { get; set; }
        public decimal StakeAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public Members Member { get; set; }
    }
}

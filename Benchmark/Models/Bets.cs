using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace Benchmark.Models
{
    [Table("Bets")]
    public partial class Bets
    {
        [Key]
        public long BetId { get; set; }
        public long MemberId { get; set; }
        public decimal StakeAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        [Write(false)]
        public Members Member { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Benchmark.Models
{
    public partial class Members
    {
        public Members()
        {
            Bets = new HashSet<Bets>();
        }

        public long MemberId { get; set; }
        public string MemberCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public ICollection<Bets> Bets { get; set; }
    }
}

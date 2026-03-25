using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class PlayerMatchResult
    {
        public Guid PlayerId { get; set; }

        public int Score { get; set; }

        public int EarnedGold { get; set; }
    }
}
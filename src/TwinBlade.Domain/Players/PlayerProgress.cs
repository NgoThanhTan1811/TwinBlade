using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class PlayerProgress
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int Gold { get; set; }

        public int HasBossCard { get; set; }

        // public int Level { get; set; } = 1;

        // public int Experience { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

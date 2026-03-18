using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class PlayerProgress
    {
        public Guid PlayerId { get; set; }

        public int Gold { get; set; }

        public List<PlayerItem> Inventory { get; set; } = new();
    }
}
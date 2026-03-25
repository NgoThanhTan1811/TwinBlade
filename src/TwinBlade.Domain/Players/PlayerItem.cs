using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwinBlade.Domain.Items;

namespace TwinBlade.Domain.Entities
{
    /// <summary>
    /// Represents items in a player's inventory (not equipped)
    /// </summary>
    public class PlayerItem
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTime AcquiredAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

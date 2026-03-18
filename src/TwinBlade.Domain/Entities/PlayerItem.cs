using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class PlayerItem
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }

        public int Quantity { get; set; }

    }
}
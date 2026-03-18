using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
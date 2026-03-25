using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Domain.Items
{
    public class Item
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty; // Format: {type}_{material} (e.g., "kiem_go")

        public Guid ItemTypeId { get; set; }
        public ItemType ItemType { get; set; } = new();

        public Guid ItemMaterialId { get; set; }
        public ItemMeterials ItemMaterial { get; set; } = new();

        public bool IsActive { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
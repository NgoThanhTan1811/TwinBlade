using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }
        public string CognitoId { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public PlayerProgress Progress { get; set; } = new();

        public List<PlayerItem> InventoryItems { get; set; } = []; // not equipped items

        public List<PlayerEquipment> EquippedItems { get; set; } = [];
    }
}

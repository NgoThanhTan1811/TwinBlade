using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Enums;
using TwinBlade.Domain.Items;

namespace TwinBlade.Domain.Entities;

/// <summary>
/// Represents items that a player currently has equipped
/// </summary>
public class PlayerEquipment
{
    public Guid Id { get; set; }

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public EquipmentSlot Slot { get; set; }

    public Guid? ItemId { get; set; }
    public Item? Item { get; set; }

    public DateTime? EquippedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

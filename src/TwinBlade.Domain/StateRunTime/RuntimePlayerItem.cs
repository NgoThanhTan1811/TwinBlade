namespace TwinBlade.Domain.Entities;

/// <summary>
/// Runtime inventory item during gameplay (stored in Redis).
/// Separate from PlayerItem (persisted in PostgreSQL).
/// </summary>
public class RuntimePlayerItem
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}

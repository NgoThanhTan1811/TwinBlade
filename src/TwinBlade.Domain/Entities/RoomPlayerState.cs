namespace TwinBlade.Domain.Entities;

/// <summary>
/// Runtime state của player trong trận đấu (lưu trong Redis)
/// </summary>
public class RoomPlayerState
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    
    // Combat stats
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int AttackPower { get; set; }
    public int Defense { get; set; }
    
    // Position in dungeon
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    
    // State flags
    public bool IsAlive { get; set; }
    public bool HasReviveCard { get; set; }
    public int ReviveCardsCount { get; set; }
    
    // Timestamps
    public DateTime LastUpdateAt { get; set; }
}

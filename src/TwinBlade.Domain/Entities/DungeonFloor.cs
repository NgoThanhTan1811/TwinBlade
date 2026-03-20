namespace TwinBlade.Domain.Entities;

/// <summary>
/// Tầng hầm ngục (persistent config)
/// </summary>
public class DungeonFloor
{
    public Guid Id { get; set; }
    public int FloorNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RecommendedLevel { get; set; }
    public bool HasBoss { get; set; }
    public int GoldReward { get; set; }
}

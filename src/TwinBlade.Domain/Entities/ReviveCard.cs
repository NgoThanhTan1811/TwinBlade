namespace TwinBlade.Domain.Entities;

/// <summary>
/// Thẻ hồi sinh trong game (persistent)
/// </summary>
public class ReviveCard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Revive Card";
    public string Description { get; set; } = "Revive a fallen teammate";
    public int HpRestorePercent { get; set; } = 50; // % HP restore khi revive
}

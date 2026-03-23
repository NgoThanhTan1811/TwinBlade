namespace TwinBlade.Domain.Entities;

/// <summary>
/// Runtime state của player trong trận đấu (lưu trong Redis)
/// </summary>
public class RoomPlayerState
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    // Timestamps
    public DateTime LastUpdateAt { get; set; }
}

using TwinBlade.Domain.Enums;

namespace TwinBlade.Domain.Entities;

/// <summary>
/// Runtime state của room đang active (lưu trong Redis)
/// </summary>
public class RoomRuntimeState
{
    public Guid RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }

    public bool BossMapActivated { get; set; }
    public bool BossDefeated { get; set; }

    // Players in game
    public List<RoomPlayerState> Players { get; set; } = new();

    // Game time
    public DateTime GameStartedAt { get; set; }
    public DateTime LastActivityAt { get; set; }

    // Metadata
    public int Version { get; set; } // Optimistic concurrency
}

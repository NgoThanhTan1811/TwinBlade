namespace TwinBlade.Application.Abstractions.Realtime;

/// <summary>
/// Typed hub interface for session events (lobby, game start)
/// NOTE: Gameplay events (HP, items, combat) are handled by Unity Mirror
/// </summary>
public interface IGameHub
{
    Task PlayerJoined(object payload);
    Task PlayerLeft(object payload);
    Task GameStarted(object payload);
}

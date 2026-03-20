namespace TwinBlade.Application.Abstractions.Realtime;

/// <summary>
/// Typed hub interface — dùng để IHubContext[GameHub, IGameHub] trong Infrastructure
/// </summary>
public interface IGameHub
{
    Task PlayerHpChanged(object payload);
    Task PlayerPickedReviveCard(object payload);
    Task PlayerRevived(object payload);
    Task RoomStateSynced(object payload);
    Task PlayerJoined(object payload);
    Task PlayerLeft(object payload);
    Task GameStarted(object payload);
    Task FloorCompleted(object payload);
}

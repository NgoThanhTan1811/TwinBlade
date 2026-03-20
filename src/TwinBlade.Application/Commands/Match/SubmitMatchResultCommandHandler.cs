using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Commands.Match;

public sealed class SubmitMatchResultCommandHandler(
    IMatchResultRepository matchResultRepository,
    IRoomRepository roomRepository,
    IPlayerRepository playerRepository)
    : IRequestHandler<SubmitMatchResultCommand, MatchResultResponse>
{
    public async Task<MatchResultResponse> Handle(SubmitMatchResultCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new InvalidOperationException("Room not found.");

        if (room.Status != RoomStatus.InGame)
            throw new InvalidOperationException("Room is not in game.");

        // Award gold to each player
        foreach (var entry in request.Players)
        {
            var player = await playerRepository.GetByIdAsync(entry.PlayerId, cancellationToken);
            if (player is not null)
                player.Progress.Gold += entry.EarnedGold;
        }

        var matchResult = new MatchResult
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            FinishedAt = DateTime.UtcNow,
            Players = request.Players.Select(p => new PlayerMatchResult
            {
                PlayerId = p.PlayerId,
                Score = p.Score,
                EarnedGold = p.EarnedGold
            }).ToList()
        };

        room.Status = RoomStatus.Finished;

        await matchResultRepository.AddAsync(matchResult, cancellationToken);
        await matchResultRepository.SaveChangesAsync(cancellationToken);
        await playerRepository.SaveChangesAsync(cancellationToken);

        return new MatchResultResponse(
            matchResult.Id,
            matchResult.RoomId,
            matchResult.FinishedAt,
            matchResult.Players.Select(p => new PlayerMatchResultResponse(p.PlayerId, p.Score, p.EarnedGold)).ToList()
        );
    }
}

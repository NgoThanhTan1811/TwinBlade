using MediatR;

namespace TwinBlade.Application.Queries.Player;

public sealed record GetAvailableAvatarsQuery : IRequest<List<string>>;

using MediatR;

namespace TwinBlade.Application.Commands.Player;

public sealed record ChangeAvatarCommand(Guid PlayerId, string AvatarFileName) : IRequest;

using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Item;

public sealed record GetItemsQuery : IRequest<List<ItemResponse>>;

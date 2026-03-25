using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Item;

public sealed record GetItemsByTypeQuery(string TypeCode) : IRequest<List<ItemResponse>>;

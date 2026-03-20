namespace TwinBlade.Application.Dtos.Response;

public sealed record ItemResponse(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl
);

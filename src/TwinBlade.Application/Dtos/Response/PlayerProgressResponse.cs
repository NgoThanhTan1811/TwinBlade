namespace TwinBlade.Application.Dtos.Response;

public sealed record PlayerProgressResponse(
    int Gold,
    List<PlayerItemResponse> Inventory
);

public sealed record PlayerItemResponse(
    Guid Id,
    Guid ItemId,
    int Quantity
);

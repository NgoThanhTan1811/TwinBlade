using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Item;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ItemController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItems(CancellationToken ct)
    {
        var items = await mediator.Send(new GetItemsQuery(), ct);
        return Ok(items);
    }
}

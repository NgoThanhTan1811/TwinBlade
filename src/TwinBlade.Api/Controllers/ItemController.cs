using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Item;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Item;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "User")]
public sealed class ItemController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItems(CancellationToken ct)
    {
        var items = await mediator.Send(new GetItemsQuery(), ct);
        return Ok(items);
    }

    [HttpGet("poison")]
    [ProducesResponseType(typeof(List<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPoisonItems(CancellationToken ct)
    {
        var items = await mediator.Send(new GetItemsByTypeQuery("poison"), ct);
        return Ok(items);
    }

    [HttpPost("poison")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePoisonItem([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        try
        {
            var description = request.Description ?? $"Poison: {request.Name}";
            var item = await mediator.Send(new CreateItemCommand(request.Code, request.Name, description), ct);
            return CreatedAtAction(nameof(GetPoisonItems), new { id = item.Id }, item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("weapon")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWeaponItem([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        try
        {
            var description = request.Description ?? $"Weapon: {request.Name}";
            var item = await mediator.Send(new CreateItemCommand(request.Code, request.Name, description), ct);
            return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

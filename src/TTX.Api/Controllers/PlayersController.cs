using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Converters;
using TTX.App.Dto.LootBoxes;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Players;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Players;
using TTX.App.Services.Transactions;
using TTX.Domain.ValueObjects;

namespace TTX.Api.Controllers;

[ApiController]
[Route("players")]
[Produces(MediaTypeNames.Application.Json)]
public class PlayersController(PlayerService playerService, TransactionService _transactionService) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetPlayers")]
    public async Task<ActionResult<PaginationDto<PlayerDto>>> Index(
        [FromQuery(Name = "page")] int index = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] PlayerOrderBy? orderBy = null,
        [FromQuery] OrderDirection? orderDir = null
    )
    {
        PaginationDto<PlayerDto> page = await playerService.Index(new IndexPlayersRequest
        {
            Page = index,
            Limit = limit,
            Search = search,
            Order = orderBy is null
                ? null
                : new Order<PlayerOrderBy>
                {
                    By = orderBy.Value,
                    Dir = orderDir ?? OrderDirection.Ascending
                },
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                Before = TimeSpan.FromDays(1)
            }
        });

        return Ok(page);
    }

    [HttpGet("{username}")]
    [EndpointName("GetPlayer")]
    public async Task<ActionResult<PlayerDto>> Show(string username, [FromQuery] TimeStep step = TimeStep.FiveMinute,
        [FromQuery] TimeSpan? before = null)
    {
        PlayerDto? player = await playerService.Find(username, new HistoryParams
        {
            Step = step,
            Before = before ?? TimeSpan.FromDays(-1)
        });

        if (player is null)
        {
            return NotFound();
        }

        return Ok(player);
    }

    [Authorize]
    [HttpGet("me")]
    [EndpointName("GetSelf")]
    public async Task<ActionResult<PlayerDto>> GetMe()
    {
        PlayerDto? player = await playerService.Find(User.FindFirstValue(ClaimTypes.Name)!, new HistoryParams
        {
            Step = TimeStep.Minute,
            Before = TimeSpan.FromDays(1)
        });

        if (player is null)
        {
            return NotFound();
        }

        return Ok(player);
    }

    [Authorize]
    [HttpPut("me/lootboxes/{lootBoxId}/open")]
    [EndpointName("Gamba")]
    public async Task<ActionResult<LootBoxResultDto>> Gamba(int lootBoxId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Result<LootBoxResultDto> result = await _transactionService.OpenLootBox(userId, lootBoxId);

        return result.ToActionResult();
    }
}

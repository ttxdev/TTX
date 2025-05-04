using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Dto.LootBoxes;
using TTX.Dto.Players;
using TTX.Dto.Transactions;
using TTX.Queries;
using TTX.Queries.Players.FindPlayer;
using TTX.Queries.Players.IndexPlayers;

namespace TTX.Api.Controllers;

[ApiController]
[Route("players")]
[Produces(MediaTypeNames.Application.Json)]
public class PlayersController(ISender sender, ISessionService sessions) : ControllerBase
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
        var page = await sender.Send(new IndexPlayersQuery
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
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        return Ok(new PaginationDto<PlayerDto>
        {
            Data = [.. page.Data.Select(PlayerDto.Create)],
            Total = page.Total
        });
    }

    [HttpGet("{username}")]
    [EndpointName("GetPlayer")]
    public async Task<ActionResult<PlayerDto>> Show(string username, [FromQuery] TimeStep step = TimeStep.FiveMinute, [FromQuery] DateTimeOffset? after = null)
    {
        var player = await sender.Send(new FindPlayerQuery
        {
            Slug = username,
            HistoryParams = new HistoryParams
            {
                Step = step,
                After = after ?? DateTimeOffset.UtcNow.AddDays(-1)
            }
        });
        if (player is null)
            return NotFound();

        return Ok(PlayerDto.Create(player));
    }

    [HttpGet("me")]
    [EndpointName("GetSelf")]
    public async Task<ActionResult<PlayerDto>> GetMe()
    {
        var username = sessions.GetCurrentUserSlug();
        if (username is null)
            return Unauthorized();

        var player = await sender.Send(new FindPlayerQuery
        {
            Slug = username,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTime.UtcNow
            }
        });

        if (player is null)
            return NotFound();

        return Ok(PlayerDto.Create(player));
    }

    [Authorize]
    [HttpPut("me/lootboxes/{lootBoxId}/open")]
    [EndpointName("Gamba")]
    public async Task<ActionResult<LootBoxResultDto>> Gamba(int lootBoxId)
    {
        var id = sessions.GetCurrentUserId();
        if (id is null)
            return Unauthorized();

        var result = await sender.Send(new OpenLootBoxCommand
        {
            ActorId = id,
            LootBoxId = lootBoxId
        });

        return Ok(LootBoxResultDto.Create(result));
    }
}

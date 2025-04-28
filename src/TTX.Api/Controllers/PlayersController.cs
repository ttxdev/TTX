using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.LootBoxes.OpenLootBox;
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
                }
        });

        return Ok(new PaginationDto<PlayerDto>
        {
            Data = [.. page.Data.Select(u => new PlayerDto(u))],
            Total = page.Total
        });
    }

    [HttpGet("{username}")]
    [EndpointName("GetPlayer")]
    public async Task<ActionResult<PlayerDto>> Show(string username)
    {
        var player = await sender.Send(new FindPlayerQuery
        {
            Slug = username
        });
        if (player is null)
            return NotFound();

        return Ok(new PlayerDto(player));
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
            Slug = username
        });

        if (player is null)
            return NotFound();

        return Ok(new PlayerDto(player));
    }

    [Authorize]
    [HttpPost]
    [EndpointName("Gamba")]
    public async Task<ActionResult<LootBoxResultDto>> Gamba()
    {
        var slug = sessions.GetCurrentUserSlug();
        if (slug is null)
            return Unauthorized();

        var result = await sender.Send(new OpenLootBoxCommand
        {
            ActorSlug = slug
        });

        return Ok(new LootBoxResultDto(result));
    }

    [HttpGet("{username}/transactions")]
    [EndpointName("GetPlayerTransactions")]
    public async Task<ActionResult<PlayerTransactionDto[]>> IndexPlayerTransactions(string username)
    {
        var player = await sender.Send(new FindPlayerQuery
        {
            Slug = username
        });

        if (player is null)
            return NotFound();


        return Ok(player.Transactions.Select(t => new PlayerTransactionDto(t)).ToArray());
    }
}
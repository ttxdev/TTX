using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Models;
using TTX.Queries;
using TTX.Queries.Players.FindPlayer;
using TTX.Queries.Players.IndexPlayers;
using TTX.ValueObjects;

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
        [FromQuery] string? searchBy = null,
        [FromQuery] string? searchValue = null,
        [FromQuery] Order[]? orders = null
    )
    {
        Search? search = searchBy != null && searchValue != null
            ? new Search
            {
                By = searchBy,
                Value = searchValue
            }
            : null;

        Pagination<Player> page = await sender.Send(new IndexPlayersQuery
        {
            Page = index,
            Limit = limit,
            Order = orders ?? [],
            Search = search,
        });

        return Ok(new PaginationDto<UserDto>
        {
            Data = [.. page.Data.Select(u => new PlayerDto(u))],
            Total = page.Total,
        });
    }

    [HttpGet("{username}")]
    [EndpointName("GetPlayer")]
    public async Task<ActionResult<PlayerDto>> Show(string username)
    {
        Player? player = await sender.Send(new FindPlayerQuery
        {
            Slug = username,
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

        Player? player = await sender.Send(new FindPlayerQuery
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
        Slug? slug = sessions.GetCurrentUserSlug();
        if (slug is null)
            return Unauthorized();

        OpenLootBoxResult result = await sender.Send(new OpenLootBoxCommand
        {
            ActorSlug = slug,
        });

        return Ok(new LootBoxResultDto(result));
    }

    [HttpGet("{username}/transactions")]
    [EndpointName("GetPlayerTransactions")]
    public async Task<ActionResult<PlayerTransactionDto[]>> IndexPlayerTransactions(string username)
    {
        Player? player = await sender.Send(new FindPlayerQuery
        {
            Slug = username,
        });

        if (player is null)
            return NotFound();


        return Ok(player.Transactions.Select(t => new PlayerTransactionDto(t)).ToArray());
    }
}

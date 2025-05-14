using System.Net.Mime;
using System.Runtime.InteropServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Interfaces;
using TTX.Commands.Creators.CreatorOptOuts;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Dto;
using TTX.Dto.Creators;
using TTX.Dto.Transactions;
using TTX.Models;
using TTX.Queries;
using TTX.Queries.Creators.FindCreator;
using TTX.Queries.Creators.IndexCreators;

namespace TTX.Api.Controllers;

[ApiController]
[Route("creators")]
[Produces(MediaTypeNames.Application.Json)]
public class CreatorsController(ISender sender, ISessionService sessions) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetCreators")]
    public async Task<ActionResult<PaginationDto<CreatorPartialDto>>> Index(
        [FromQuery(Name = "page")] int index = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] CreatorOrderBy? orderBy = null,
        [FromQuery] OrderDirection? orderDir = null
    )
    {
        var page = await sender.Send(new IndexCreatorsQuery
        {
            Page = index,
            Limit = limit,
            Order = orderBy is null
                ? null
                : new Order<CreatorOrderBy>
                {
                    By = orderBy.Value,
                    Dir = orderDir ?? OrderDirection.Ascending
                },
            Search = search,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        return Ok(page);
    }

    [HttpGet("{slug}")]
    [EndpointName("GetCreator")]
    public async Task<ActionResult<CreatorDto>> Show(string slug, [FromQuery] TimeStep step = TimeStep.FiveMinute,
        [FromQuery] DateTimeOffset? after = null)
    {
        var creator = await sender.Send(new FindCreatorQuery
        {
            Slug = slug,
            HistoryParams = new HistoryParams
            {
                Step = step,
                After = after ?? DateTimeOffset.UtcNow.AddDays(-1)
            }
        });

        if (creator is null)
            return NotFound();

        return Ok(creator);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateCreator")]
    public async Task<ActionResult<CreatorDto>> Create([FromQuery] string username, [FromQuery] string ticker)
    {
        var creator = await sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = ticker,
            Username = username
        });

        return Ok(creator);
    }

    [HttpGet("{creatorSlug}/transactions")]
    [EndpointName("GetCreatorTransactions")]
    public async Task<ActionResult<PlayerTransactionDto[]>> IndexCreatorTransactions(string slug)
    {
        var creator = await sender.Send(new FindCreatorQuery
        {
            Slug = slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTimeOffset.UtcNow
            }
        });

        if (creator is null)
            return NotFound();

        return Ok(creator);
    }

    [HttpDelete("{creatorSlug}")]
    [EndpointName("CreatorOptOut")]
    public async Task<ActionResult<CreatorOptOutDto>> CreatorOptOut(
        string creatorSlug
    )
    {
        var curUser = sessions.GetCurrentUserSlug();

        if (curUser is null || curUser.Value != creatorSlug)
        {
            return Unauthorized("Current user is not the creator");
        }

        var res = await sender.Send(new CreatorOptOutCommand
        {
            Username = creatorSlug
        });

        return Ok(res);
    }
}

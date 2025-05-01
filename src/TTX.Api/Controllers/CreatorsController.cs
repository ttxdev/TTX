using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Dto.Creators;
using TTX.Dto.Transactions;
using TTX.Models;
using TTX.Queries;
using TTX.Queries.Creators;
using TTX.Queries.Creators.FindCreator;
using TTX.Queries.Creators.IndexCreators;
using TTX.Queries.Creators.PullLatestHistory;
using TTX.ValueObjects;

namespace TTX.Api.Controllers;

[ApiController]
[Route("creators")]
[Produces(MediaTypeNames.Application.Json)]
public class CreatorsController(ISender sender) : ControllerBase
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

        return Ok(new PaginationDto<CreatorPartialDto>
        {
            Data = [.. page.Data.Select(CreatorPartialDto.Create)],
            Total = page.Total
        });
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

        return Ok(CreatorDto.Create(creator));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateCreator")]
    public async Task<ActionResult<CreatorDto>> Create([FromQuery] string username, [FromQuery] Ticker ticker)
    {
        var creator = await sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = ticker,
            Username = username
        });

        return Ok(CreatorDto.Create(creator));
    }

    [HttpGet("{creatorSlug}/value/latest")]
    [EndpointName("GetLatestCreatorValue")]
    public async Task<ActionResult<Vote[]>> GetLatestValues(
        [FromRoute] string creatorSlug,
        [FromQuery] DateTime after,
        [FromQuery] TimeStep step = TimeStep.Minute
    )
    {
        var votes = await sender.Send(new PullLatestHistoryQuery
        {
            CreatorSlug = creatorSlug,
            Step = step,
            After = after
        });

        return Ok(votes);
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

        return Ok(creator.Transactions.Select(CreatorTransactionDto.Create).ToArray());
    }
}
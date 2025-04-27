using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Commands.Creators.OnboardTwitchCreator;
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

        var page = await sender.Send(new IndexCreatorsQuery
        {
            Page = index,
            Limit = limit,
            Order = orders ?? [],
            Search = search,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
        });

        return Ok(new PaginationDto<CreatorPartialDto>
        {
            Data = [.. page.Data.Select(c => new CreatorPartialDto(c))],
            Total = page.Total,
        });
    }

    [HttpGet("{slug}")]
    [EndpointName("GetCreator")]
    public async Task<ActionResult<CreatorDto>> Show(string slug, [FromQuery] TimeStep step = TimeStep.FiveMinute, [FromQuery] DateTimeOffset? after = null)
    {
        Creator? creator = await sender.Send(new FindCreatorQuery
        {
            Slug = slug,
            HistoryParams = new HistoryParams
            {
                Step = step,
                After = after ?? DateTimeOffset.UtcNow.AddDays(-1),
            }
        });

        if (creator is null)
            return NotFound();

        return Ok(new CreatorDto(creator));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateCreator")]
    public async Task<ActionResult<CreatorDto>> Create([FromQuery] string username, [FromQuery] Ticker ticker)
    {
        Creator creator = await sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = ticker,
            Username = username,
        });

        return Ok(new CreatorDto(creator));
    }

    [HttpGet("{creatorSlug}/value/latest")]
    [EndpointName("GetLatestCreatorValue")]
    public async Task<ActionResult<Vote[]>> GetLatestValues(
      [FromRoute] string creatorSlug,
      [FromQuery] DateTime after,
      [FromQuery] TimeStep step = TimeStep.Minute
    )
    {
        Vote[] votes = await sender.Send(new PullLatestHistoryQuery
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
        Creator? creator = await sender.Send(new FindCreatorQuery
        {
            Slug = slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTimeOffset.UtcNow,
            }
        });

        if (creator is null)
            return NotFound();


        return Ok(creator.Transactions.Select(t => new CreatorTransactionDto(t)).ToArray());
    }
}

using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Converters;
using TTX.App.Dto.Creators;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Creators;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Api.Controllers;

[ApiController]
[Route("creators")]
[Produces(MediaTypeNames.Application.Json)]
public class CreatorsController(CreatorService _creatorService) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetCreators")]
    public async Task<ActionResult<PaginationDto<CreatorDto>>> Index(
        [FromQuery(Name = "page")] int index = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] CreatorOrderBy? orderBy = null,
        [FromQuery] OrderDirection? orderDir = null
    )
    {
        PaginationDto<CreatorDto> page = await _creatorService.Index(new IndexCreatorsRequest
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
                Before = TimeSpan.FromDays(1)
            }
        });

        return Ok(page);
    }

    [HttpGet("{slug}")]
    [EndpointName("GetCreator")]
    public async Task<ActionResult<CreatorDto>> Show(
        string slug,
        [FromQuery] TimeSpan? before = null)
    {
        before ??= TimeSpan.FromDays(1);

        CreatorDto? creator = await _creatorService.Find(slug, new HistoryParams
        {
            Before = before.Value,
            Step = before.Value.Days switch
                {
                    > 30 => TimeStep.Month,
                    > 7 => TimeStep.Week,
                    > 1 => TimeStep.Day,
                    _ => before.Value.TotalHours > 1 ? TimeStep.Hour : TimeStep.Minute
                }
        });

        if (creator is null)
        {
            return NotFound();
        }

        return Ok(creator);
    }

    [HttpPost]
    [Authorize(Roles = nameof(PlayerType.Admin))]
    [EndpointName("CreateCreator")]
    public async Task<ActionResult<ModelId>> Create([FromQuery] string username, [FromQuery] string ticker)
    {
        Result<ModelId> result = await _creatorService.Onboard(new OnboardRequest
        {
            Ticker = ticker,
            Username = username,
            Platform = Platform.Twitch
        });

        return result.ToActionResult();
    }

    [Authorize]
    [HttpDelete("{slug}")]
    [EndpointName("OptOutCreator")]
    public async Task<ActionResult<CreatorOptOutDto>> OptOut(
        string slug,
        [FromQuery] string? reason = null
    )
    {
        ModelId playerId = ModelId.Create(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PlayerType role = Enum.Parse<PlayerType>(User.FindFirstValue(ClaimTypes.Role)!);
        if (role != PlayerType.Admin && !await _creatorService.IsPlayer(slug, playerId))
        {
            return Unauthorized();
        }

        Result<CreatorOptOutDto> result = await _creatorService.OptOut(slug, reason ?? string.Empty);
        return result.ToActionResult();
    }
}

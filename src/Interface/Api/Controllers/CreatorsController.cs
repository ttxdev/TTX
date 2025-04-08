using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using TTX.Core.Services;
using TTX.Core.Models;
using TTX.Core;
using TTX.Core.Repositories;
using TTX.Interface.Api.Dto;
using TTX.Interface.Api.Services;

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("creators")]
[Produces(MediaTypeNames.Application.Json)]
public class CreatorsController(SessionService sessionService, IOrderService orderService, ICreatorService creatorService) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetCreators")]
    public async Task<ActionResult<Pagination<CreatorPartialDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? searchBy = null,
        [FromQuery] string? searchValue = null,
        [FromQuery] Order[]? orders = null
    )
    {
        var search = searchBy != null && searchValue != null
            ? new Search
            {
                By = searchBy,
                Value = searchValue
            }
            : null;

        var dataPage = await creatorService.GetPaginated(
            page,
            limit,
            orders,
            search
        );

        return Ok(new Pagination<CreatorPartialDto>
        {
            Data = [.. dataPage.Data.Select(c => new CreatorPartialDto(c))],
            Total = dataPage.Total,
        });
    }

    [HttpGet("{slug}")]
    [EndpointName("GetCreator")]
    public async Task<ActionResult<CreatorDto>> Get(string slug)
    {
        var creator = await creatorService.GetDetails(slug);
        if (creator is null)
            return NotFound();

        return new CreatorDto(creator);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateCreator")]
    public async Task<ActionResult<CreatorDto>> Create([FromQuery] string username, [FromQuery] string ticker)
    {
        try
        {
            return Ok(new CreatorDto(await creatorService.Onboard(username, ticker)));
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{creatorSlug}/shares")]
    [EndpointName("GetCreatorShares")]
    public async Task<ActionResult<CreatorShareDto[]>> GetCreatorShares(string creatorSlug)
    {
        var creator = await creatorService.GetDetails(creatorSlug);
        if (creator is null)
            return NotFound("Creator not found.");

        return Ok(creator.GetShares().Select(s => new CreatorShareDto(s)).ToArray());
    }

    [HttpGet("{creatorSlug}/transactions")]
    [EndpointName("GetCreatorTransactions")]
    public async Task<ActionResult<CreatorTransactionDto[]>> GetTransactions(string creatorSlug)
    {
        var creator = await creatorService.GetDetails(creatorSlug);
        if (creator == null)
            return NotFound("Creator not found.");

        return Ok(creator.Transactions.Select(t => new CreatorTransactionDto(t)).ToArray());
    }

    [Authorize]
    [HttpPost("{creatorSlug}/transactions")]
    [EndpointName("CreateTransaction")]
    public async Task<ActionResult<CreatorTransactionDto>> PlaceOrder(string creatorSlug, [FromQuery] TransactionAction action, [FromQuery] int amount)
    {
        if (sessionService.CurrentUserSlug is null)
            return Unauthorized();

        try
        {
            var tx = await orderService.PlaceOrder(
                username: sessionService.CurrentUserSlug,
                creatorSlug: creatorSlug,
                action, amount
            );

            return Ok(new CreatorTransactionDto(tx));
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{creatorSlug}/value/latest")]
    [EndpointName("GetLatestCreatorValue")]
    public async Task<ActionResult<Vote[]>> GetLatestValues(
      [FromRoute] string creatorSlug,
      [FromQuery] DateTime after,
      [FromQuery] TimeStep step = TimeStep.Minute
    )
    {
        var votes = await creatorService.PullLatestHistory(creatorSlug, after, step);
        if (votes is null)
            return NotFound("Creator not found.");

        return Ok(votes);
    }
}
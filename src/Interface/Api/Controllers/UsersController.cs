using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;
using TTX.Core.Services;
using TTX.Interface.Api.Dto;

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("users")]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController(ISessionService sessionService, IUserService userService) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetUsers")]
    public async Task<ActionResult<Pagination<UserDto>>> GetAll(
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

        var dataPage = await userService.GetPaginated(
            page,
            limit,
            orders,
            search
        );

        return Ok(new Pagination<UserDto>
        {
            Data = [.. dataPage.Data.Select(u => new UserDto(u))],
            Total = dataPage.Total,
        });
    }

    [HttpGet("me")]
    [EndpointName("GetSelf")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await sessionService.GetUser();
        if (user is null)
            return NotFound();

        return Ok(new UserDto(user));
    }

    [HttpGet("{username}")]
    [EndpointName("GetUser")]
    public async Task<ActionResult<UserDto>> Get(string username)
    {
        var user = await userService.GetDetails(username);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserDto(user));
    }

    [Authorize]
    [HttpPost]
    [EndpointName("Gamba")]
    public async Task<ActionResult<LootBoxResultDto>> Gamba()
    {
        var result = await userService.Gamba();
        return Ok(new LootBoxResultDto(result));
    }

    [HttpGet("{username}/transactions")]
    [EndpointName("GetUserTransactions")]
    public async Task<ActionResult<UserTransactionDto[]>> GetUserTransactions(string username)
    {
        var user = await userService.GetDetails(username);
        if (user is null)
            return NotFound();

        return Ok(user.Transactions.Select(t => new UserTransactionDto(t)).ToArray());
    }
}
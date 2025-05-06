using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Dto.Transactions;

namespace TTX.Api.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController(ISender sender, ISessionService sessions) : Controller
{
    [Authorize]
    [HttpPost]
    [EndpointName("PlaceOrder")]
    public async Task<ActionResult<CreatorTransactionDto>> Create([FromBody] CreateTransactionDto order)
    {
        var actorId = sessions.GetCurrentUserId();
        if (actorId is null)
            return Unauthorized();

        var tx = await sender.Send(new PlaceOrderCommand
        {
            ActorId = actorId,
            Creator = order.CreatorSlug,
            Action = order.Action,
            Amount = order.Amount
        });

        return Ok(tx);
    }
}

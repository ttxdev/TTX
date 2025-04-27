using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Models;
using TTX.ValueObjects;

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
        Slug? slug = sessions.GetCurrentUserSlug();
        if (slug is null)
            return Unauthorized();

        Transaction tx = await sender.Send(new PlaceOrderCommand
        {
            Actor = slug,
            Creator = order.CreatorSlug,
            Action = order.Action,
            Amount = order.Amount
        });

        return Ok(new CreatorTransactionDto(tx));
    }
}

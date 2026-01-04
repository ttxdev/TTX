using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TTX.Api.Controllers.Dto;
using TTX.Api.Converters;
using TTX.App.Services.Transactions;
using TTX.Domain.ValueObjects;

namespace TTX.Api.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController(TransactionService _txService) : Controller
{
    [Authorize]
    [HttpPost]
    [EnableRateLimiting("TransactionRateLimiter")]
    [EndpointName("PlaceOrder")]
    public async Task<ActionResult<ModelId>> Create([FromBody] CreateTransactionDto order)
    {
        Result<ModelId> result = await _txService.PlaceOrder(
            actorId: int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            creatorSlug: order.CreatorSlug,
            action: order.Action,
            quantity: order.Quantity
        );

        return result.ToActionResult();
    }
}

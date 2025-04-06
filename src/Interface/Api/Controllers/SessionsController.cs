using Microsoft.AspNetCore.Mvc;
using TTX.Core;
using TTX.Core.Services;
using TTX.Interface.Api.Dto;
using TTX.Interface.Api.Services;

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(SessionService sessionService, IUserService userService) : ControllerBase
{
    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code)
    {
        try
        {
            var user = await userService.OAuthOnboard(code);
            return new TokenDto { Token = sessionService.CreateSession(user) };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }
}
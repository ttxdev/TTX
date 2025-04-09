using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TTX.Core;
using TTX.Core.Services;
using TTX.Interface.Api.Dto;
using TTX.Interface.Api.Services;

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(SessionService sessionService, IIdentityService identityService) : ControllerBase
{
    [HttpGet("login")]
    [SwaggerIgnore]
    public ActionResult Login() => Redirect(sessionService.GetTwitchLoginUrl());

    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code)
    {
        try
        {
            var user = await identityService.ProcessOAuth(code);
            return new TokenDto { Token = sessionService.CreateSession(user) };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }
}
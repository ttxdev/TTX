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

    [HttpGet("discord/callback")]
    [EndpointName("DiscordCallback")]
    public async Task<ActionResult<DiscordDto>> DiscordCallback([FromQuery] string code)
    {
        try
        {
            var user = await identityService.ProcessDiscordOAuth(code);
            return new DiscordDto
            {
                Token = user.access_token,
                Users = user.TwitchUsers.Select(u => new DiscordTwitchDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Login = u.Login,
                    AvatarUrl = u.AvatarUrl
                }).ToArray()
            };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("discord/callback/twitch")]
    [EndpointName("DiscordCallbackToTwitch")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code, [FromQuery] string twitchId)
    {
        try
        {
            var user = await identityService.ProcessDiscordToTwitchOAuth(code, twitchId);
            return new TokenDto { Token = sessionService.CreateSession(user) };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }

}
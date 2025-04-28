using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Commands.Players.AuthenticateTwitchUser;
using TTX.Commands.Players.OnboardTwitchUser;
using TTX.Models;

namespace TTX.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(ISender sender, ISessionService sessions) : ControllerBase
{
    [HttpGet("login")]
    [SwaggerIgnore]
    public ActionResult Login() => Redirect(sessions.GetTwitchLoginUrl());

    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code)
    {
        Player player = await sender.Send(new AuthenticateTwitchUserCommand
        {
            OAuthCode = code
        });
        string token = sessions.CreateSession(player);

        return new TokenDto(token);
    }

    [HttpGet("discord/callback")]
    [EndpointName("DiscordCallback")]
    public async Task<ActionResult<TokenDto>> DiscordCallback([FromQuery] string code)
    {
        var result = await sender.Send(new AuthenticateDiscordUserCommand
        {
            OAuthCode = code
        });
        
        return Ok(new TokenDto(sessions.CreateDiscordSession(result)));
    }
    
    [HttpPost("discord/link")]
    [EndpointName("LinkDiscordTwitch")]
    public async Task<ActionResult<TokenDto>> DiscordCallback([FromBody] LinkDiscordTwitchDto req)
    {
        TwitchUserDto? tUser = sessions.ParseDiscordSession(req.Token).FirstOrDefault(t => t.Id == req.TwitchId);
        if (tUser is null)
            return NotFound();

        return Ok(await sender.Send(new OnboardTwitchUserCommand
        {
            
            Id = tUser.Id
        }).ContinueWith(t => new TokenDto(sessions.CreateSession(t.Result))));
    }
}

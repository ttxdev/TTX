using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Commands.Players.AuthenticateTwitchUser;
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
    public async Task<ActionResult<AuthenticateDiscordUserDto>> DiscordCallback([FromQuery] string code)
    {
        AuthenticateDiscordUserResult result = await sender.Send(new AuthenticateDiscordUserCommand
        {
            OAuthCode = code
        });

        return new AuthenticateDiscordUserDto(result);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using TTX.Core;
using TTX.Core.Models;
using TTX.Core.Services;
using TTX.Interface.Api.Dto;
using TTX.Interface.Api.Provider;
using TTX.Interface.Api.Services;

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(SessionService sessionService, IUserService userService) : ControllerBase
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
            var user = await userService.ProcessOAuth(code);
            return new TokenDto { Token = sessionService.CreateSession(user) };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }
}
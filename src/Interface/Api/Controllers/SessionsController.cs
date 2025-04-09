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

namespace TTX.Interface.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(IConfigProvider config, IUserService userService) : ControllerBase
{
    [HttpGet("login")]
    [SwaggerIgnore]
    public ActionResult Login()
    {
        var redirectUri = config.GetTwitchRedirectUri();
        var clientId = config.GetTwitchClientId();
        var scope = "";
        var state = Guid.NewGuid().ToString();
        var url = $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
        return Redirect(url);
    }


    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code)
    {
        try
        {
            var user = await userService.ProcessOAuth(code);
            return new TokenDto { Token = CreateSession(user) };
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
            var user = await userService.ProcessDiscordOAuth(code);
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
            var user = await userService.ProcessDiscordToTwitchOAuth(code, twitchId);
            return new TokenDto { Token = CreateSession(user) };
        }
        catch (DomainException e)
        {
            return BadRequest(e.Message);
        }
    }

    private string CreateSession(User user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim("AvatarUrl", user.AvatarUrl),
        new Claim(ClaimTypes.Role, user.Type.ToString()),
        new Claim("UpdatedAt", user.UpdatedAt.ToString("o"))
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSecretKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
          issuer: "ttx.gg",
          audience: "ttx.gg",
          claims: claims,
          expires: DateTime.Now.AddDays(7),
          signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
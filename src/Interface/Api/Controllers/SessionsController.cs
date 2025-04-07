using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code)
    {
        try
        {
            var user = await userService.OAuthOnboard(code);
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
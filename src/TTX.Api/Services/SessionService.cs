using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TTX.Api.Interfaces;
using TTX.Api.Provider;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Api.Services;

public class SessionService(
    IConfigProvider config,
    IHttpContextAccessor httpContextAccessor
) : ISessionService
{
    public Slug? GetCurrentUserSlug()
    {
        var val = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
        if (val is null)
            return null;

        return val;
    }

    public string GetTwitchLoginUrl()
    {
        var redirectUri = config.GetTwitchRedirectUri();
        var clientId = config.GetTwitchClientId();
        var scope = "";
        var state = Guid.NewGuid().ToString();

        return $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public string CreateSession(Player player)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.Name, player.Name),
            new Claim("AvatarUrl", player.AvatarUrl.ToString()),
            new Claim(ClaimTypes.Role, player.Type.ToString()),
            new Claim("UpdatedAt", player.UpdatedAt.ToString("o"))
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
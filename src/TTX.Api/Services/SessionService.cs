using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using TTX.Api.Dto;
using TTX.Api.Interfaces;
using TTX.Api.Provider;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Dto.Players;
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
        return val ?? null;
    }

    public ModelId? GetCurrentUserId()
    {
        var val = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return val is null ? null : ModelId.Create(int.Parse(val));
    }

    public string GetTwitchLoginUrl()
    {
        var redirectUri = config.GetTwitchRedirectUri();
        var clientId = config.GetTwitchClientId();
        var scope = "";
        var state = Guid.NewGuid().ToString();

        return
            $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public string CreateSession(PlayerPartialDto player)
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
            issuer: "api.ttx.gg",
            audience: "ttx.gg",
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateDiscordSession(AuthenticateDiscordUserResult result)
    {
        var claims = new[]
        {
            new Claim("connections", JsonSerializer.Serialize(
                result.TwitchUsers.Select(u => TwitchUserDto.Create(u))
            ))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSecretKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "api.ttx.gg",
            audience: "discord.ttx.gg",
            claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TwitchUserDto[] ParseDiscordSession(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config.GetSecretKey());

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "api.ttx.gg",
            ValidAudience = "discord.ttx.gg",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

        var tUsers = JsonSerializer.Deserialize<TwitchUserDto[]>(principal.FindFirstValue("connections")!);
        return tUsers!;
    }
}

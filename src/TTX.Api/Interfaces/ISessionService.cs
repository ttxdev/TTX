using TTX.Api.Dto;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Api.Interfaces;

public interface ISessionService
{
    public Slug? GetCurrentUserSlug();
    public string GetTwitchLoginUrl();
    public string CreateSession(Player player);
    public string CreateDiscordSession(AuthenticateDiscordUserResult user);
    public TwitchUserDto[] ParseDiscordSession(string token);
}
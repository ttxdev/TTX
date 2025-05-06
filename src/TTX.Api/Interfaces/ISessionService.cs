using TTX.Api.Dto;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Dto.Players;
using TTX.ValueObjects;

namespace TTX.Api.Interfaces;

public interface ISessionService
{
    public Slug? GetCurrentUserSlug();
    public ModelId? GetCurrentUserId();
    public string GetTwitchLoginUrl();
    public string CreateSession(PlayerPartialDto player);
    public string CreateDiscordSession(AuthenticateDiscordUserResult user);
    public TwitchUserDto[] ParseDiscordSession(string token);
}
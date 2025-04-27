namespace TTX.Commands.Players.AuthenticateDiscordUser;

public readonly struct AuthenticateDiscordUserCommand : ICommand<AuthenticateDiscordUserResult>
{
    public required string OAuthCode { get; init; }
}

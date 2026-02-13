namespace TTX.Infrastructure.Options;

public record TwitchChatOptions
{
    public string Username { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}

namespace TTX.Infrastructure.Options;

public sealed class TwitchOAuthOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
    public string[] Scopes { get; init; } = [];
}

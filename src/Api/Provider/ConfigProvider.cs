namespace TTX.Interface.Api.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
    string GetSecretKey();
    string GetTwitchClientId();
    string GetTwitchClientSecret();
    string GetTwitchRedirectUri();
    string GetDiscordClientId();
    string GetDiscordClientSecret();
}

public class ConfigProvider(IConfiguration configuration) : IConfigProvider
{
    public string GetConnectionString() => configuration["CONNECTION_STRING"]!;
    public string GetSecretKey() => configuration["SECRET_KEY"]!;
    public string GetTwitchClientId() => configuration["TWITCH_CLIENT_ID"]!;
    public string GetTwitchClientSecret() => configuration["TWITCH_CLIENT_SECRET"]!;
    public string GetTwitchRedirectUri() => configuration["TWITCH_REDIRECT_URI"]!;
    public string GetDiscordClientId() => configuration["DISCORD_CLIENT_ID"]!;
    public string GetDiscordClientSecret() => configuration["DISCORD_CLIENT_SECRET"]!;

    public override string ToString()
    {
        return $"ConnectionString: {GetConnectionString()}\nSecretKey: {GetSecretKey()}\nTwitchClientId: {GetTwitchClientId()}\nTwitchClientSecret: {GetTwitchClientSecret()}\nTwitchRedirectUrl: {GetTwitchRedirectUri()}";
    }
}
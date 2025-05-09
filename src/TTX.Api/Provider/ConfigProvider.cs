namespace TTX.Api.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
    string GetSecretKey();
    string GetRedisConnectionString();
    string GetTwitchClientId();
    string GetTwitchClientSecret();
    string GetTwitchRedirectUri();
    string GetDiscordClientId();
    string GetDiscordClientSecret();
}

public class ConfigProvider(IConfiguration configuration) : IConfigProvider
{
    public string GetConnectionString()
    {
        return configuration["CONNECTION_STRING"]!;
    }

    public string GetRedisConnectionString()
    {
        return configuration["REDIS_URL"]!;
    }

    public string GetSecretKey()
    {
        return configuration["SECRET_KEY"]!;
    }

    public string GetTwitchClientId()
    {
        return configuration["TWITCH_CLIENT_ID"]!;
    }

    public string GetTwitchClientSecret()
    {
        return configuration["TWITCH_CLIENT_SECRET"]!;
    }

    public string GetTwitchRedirectUri()
    {
        return configuration["TWITCH_REDIRECT_URI"]!;
    }

    public string GetDiscordClientId()
    {
        return configuration["DISCORD_CLIENT_ID"]!;
    }

    public string GetDiscordClientSecret()
    {
        return configuration["DISCORD_CLIENT_SECRET"]!;
    }
}
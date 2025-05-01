using Microsoft.Extensions.Configuration;

namespace TTX.StreamMonitor.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
    string GetRedisConnectionString();
    string GetTwitchClientId();
    string GetTwitchClientSecret();
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

    public string GetTwitchClientId()
    {
        return configuration["TWITCH_CLIENT_ID"]!;
    }

    public string GetTwitchClientSecret()
    {
        return configuration["TWITCH_CLIENT_SECRET"]!;
    }
}
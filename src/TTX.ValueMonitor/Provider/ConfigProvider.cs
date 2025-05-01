using Microsoft.Extensions.Configuration;

namespace TTX.ValueMonitor.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
    string GetRedisConnectionString();
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
}
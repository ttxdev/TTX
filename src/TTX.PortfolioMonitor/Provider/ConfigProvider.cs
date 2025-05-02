using Microsoft.Extensions.Configuration;

namespace TTX.PortfolioMonitor.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
    string GetRedisConnectionString();
    int GetBuffer();
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

    public int GetBuffer()
    {
        return int.Parse(configuration["BUFFER"]!);
    }
}
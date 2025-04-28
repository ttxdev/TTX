using Microsoft.Extensions.Configuration;

namespace TTX.ValueMonitor.Provider;

public interface IConfigProvider
{
    string GetConnectionString();
}

public class ConfigProvider(IConfiguration configuration) : IConfigProvider
{
    public string GetConnectionString()
    {
        return configuration["CONNECTION_STRING"]!;
    }
}
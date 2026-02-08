using Microsoft.Extensions.DependencyInjection;
using TTX.App.Interfaces.Chat;
using TTX.Domain.Models;

namespace TTX.App.Factories;

public class ChatMonitorFactory(IServiceProvider services)
{
    private readonly Dictionary<Platform, IChatMonitorAdapter> _instances = Enum.GetValues<Platform>().ToDictionary(
            p => p,
            p => services.GetRequiredKeyedService<IChatMonitorAdapter>(p)
        );

    public IChatMonitorAdapter Create(Platform platform) => _instances[platform];
    public IEnumerable<IChatMonitorAdapter> CreateAll() => _instances.Values;
}

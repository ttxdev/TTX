using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;
using TTX.Infrastructure.Data;
using TTX.Infrastructure.Data.Repositories;
using TTX.Interface.StreamMonitor.Provider;
using TTX.Infrastructure.Twitch.Services;
using dotenv.net;
using TTX.Interface.StreamMonitor.Services;

DotEnv.Load();
var config = new ConfigProvider(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
var serviceProvider = new ServiceCollection()
  .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
  .AddSingleton<IConfigProvider>(config)
  .AddLogging(options => options.AddConsole())
  .AddScoped<ICreatorRepository, CreatorRepository>()
  .BuildServiceProvider();

var monitor = new StreamMonitorService(
  serviceProvider, 
  new TwitchStreamService(config.GetTwitchClientId(), config.GetTwitchClientSecret()),
  serviceProvider.GetRequiredService<ILogger<StreamMonitorService>>()
);
var creators = new List<Creator>();
using var scope = serviceProvider.CreateAsyncScope();
{
  var creatorRepository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
  foreach (var creator in await creatorRepository.GetAll())
      monitor.AddCreator(creator);
}

await monitor.Start(CancellationToken.None);

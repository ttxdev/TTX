using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;
using TTX.Core.Services;
using TTX.Infrastructure.Data;
using TTX.Infrastructure.Data.Repositories;
using TTX.Interface.StreamMonitor.Provider;
using TTX.Interface.StreamMonitor.Services;
using dotenv.net;

DotEnv.Load();
var config = new ConfigProvider(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
var serviceProvider = new ServiceCollection()
  .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
  .AddLogging(options => options.AddConsole())
  .AddSingleton<IConfigProvider>(config)
  .AddScoped<ICreatorRepository, CreatorRepository>()
  .AddScoped<IVoteRepository, VoteRepository>()
  .AddScoped<IStreamService, TwitchStreamMonitor>()
  .AddScoped<IStreamMonitorService, StreamMonitorService>()
  .BuildServiceProvider();

var creators = new List<Creator>();
using var scope = serviceProvider.CreateScope();
var monitor = scope.ServiceProvider.GetRequiredService<IStreamMonitorService>();
var creatorRepository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
foreach (var creator in await creatorRepository.GetAll())
    monitor.AddCreator(creator);

await monitor.Start(CancellationToken.None);

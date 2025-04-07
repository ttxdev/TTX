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
using TTX.Interface.StreamMonitor.Services;

var config = new ConfigProvider(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
var serviceProvider = new ServiceCollection()
  .AddLogging(options => options.AddConsole())
  .AddSingleton<IConfigProvider>(config)
  .AddScoped<ICreatorRepository, CreatorRepository>()
  .AddScoped<IVoteRepository, VoteRepository>()
  .AddSingleton<IStreamService, TwitchStreamMonitor>()
  .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
  .BuildServiceProvider();

var creators = new List<Creator>();
using var scope = serviceProvider.CreateScope();
var streamService = scope.ServiceProvider.GetRequiredService<IStreamService>();
var creatorRepository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
foreach (var creator in await creatorRepository.GetAll())
  streamService.AddCreator(creator);

await streamService.Start(CancellationToken.None);

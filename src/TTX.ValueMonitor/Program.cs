using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Infrastructure.Data;
using TTX.ValueMonitor.Provider;
using TTX.ValueMonitor.Services;

DotEnv.Load();
ConfigProvider config = new(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
IServiceProvider services = new ServiceCollection()
  .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
  .AddLogging(options =>
  {
      options.AddConsole();
  })
  .AddMediatR(cfg =>
  {
      cfg.RegisterServicesFromAssemblyContaining<UpdateStreamStatusHandler>();
  })
  .BuildServiceProvider();

ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
TwitchBotService bot = new(services, loggerFactory.CreateLogger<TwitchBotService>());
using AsyncServiceScope scope = services.CreateAsyncScope();
{
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Creators.ForEachAsync(bot.AddCreator);
}

await bot.Start(CancellationToken.None);

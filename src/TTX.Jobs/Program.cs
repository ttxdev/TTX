using Microsoft.Extensions.DependencyInjection.Extensions;
using TTX.App;
using TTX.App.Jobs.CreatorValues;
using TTX.Infrastructure;
#if BOT_EXISTS
using TTX.Bot;
#endif

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddTtx(configuration.GetSection("TTX"))
    .AddTtxJobs(configuration.GetSection("TTX:Jobs"))
    .AddTtxInfra(configuration.GetSection("TTX:Infrastructure"))
    .BuildServiceProvider();

IHost host = builder.Build();
host.Run();

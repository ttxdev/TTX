using TTX.App;
using TTX.Infrastructure;

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

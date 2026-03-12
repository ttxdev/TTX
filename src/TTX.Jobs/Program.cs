using TTX.App;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TTX.Infrastructure;
using OpenTelemetry.Resources;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddLogging(opt =>
        {
            opt.AddOpenTelemetry();
            if (!builder.Environment.IsProduction())
            {
                opt.AddConsole();
            }
        })
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddHostDetector().AddContainerDetector())
    .WithLogging(logging => logging.AddOtlpExporter())
    .WithTracing(tracing => tracing.AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddOtlpExporter())
    .Services
    .AddTtx(configuration.GetSection("TTX"))
    .AddTtxJobs(configuration.GetSection("TTX:Jobs"))
    .AddTtxInfra(configuration.GetSection("TTX:Infrastructure"))
    .BuildServiceProvider();

IHost host = builder.Build();
host.Run();

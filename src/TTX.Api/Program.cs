using System.Text;
using System.Text.Json.Serialization;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TTX;
using TTX.Api.Hubs;
using TTX.Api.Interfaces;
using TTX.Api.Middleware;
using TTX.Api.Notifications;
using TTX.Api.Provider;
using TTX.Api.Services;
using TTX.Infrastructure.Data;
using TTX.Infrastructure.Discord;
using TTX.Infrastructure.Twitch;
using TTX.Interfaces.Discord;
using TTX.Interfaces.Twitch;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment()) DotEnv.Load();

builder.Configuration.AddEnvironmentVariables("TTX_");
IConfigProvider config = new ConfigProvider(builder.Configuration);

// Add services to the container.
builder.Services
    .Configure<JsonOptions>(o => { o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); })
    .AddLogging(options =>
    {
        options.AddConsole();
        options.AddDebug();
    })
    .AddLogging(cfg =>
    {
        cfg.AddConsole();
        cfg.AddDebug();
    })
    .AddHttpLogging()
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddSingleton<CreateTransactionNotificationHandler>()
    .AddSingleton<UpdateCreatorValueNotificationHandler>()
    .AddSingleton<UpdatePlayerPortfolioNotificationHandler>()
    .AddSingleton(config)
    .AddDbContextPool<ApplicationDbContext>(
        options =>
        {
            options.UseNpgsql(config.GetConnectionString());
            options.EnableDetailedErrors();
        },
        700
    )
    .AddHttpContextAccessor()
    .AddSwaggerGen(options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();
    })
    .AddTransient<ITwitchAuthService, TwitchAuthService>(_ => new TwitchAuthService(
        config.GetTwitchClientId(),
        config.GetTwitchClientSecret(),
        config.GetTwitchRedirectUri()
    ))
    .AddTransient<IDiscordAuthService, DiscordAuthService>(_ => new DiscordAuthService(
        config.GetDiscordClientId(),
        config.GetDiscordClientSecret()
    ))
    .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.GetRedisConnectionString()))
    .AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<Program>();
        cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>();
    })
    .AddHostedService<UpdateCreatorValueNotificationHandler>()
    .AddHostedService<UpdatePlayerPortfolioNotificationHandler>()
    .AddHostedService<CreateTransactionNotificationHandler>()
    .AddHostedService<UpdateStreamStatusNotificationHandler>()
    .AddTransient<ISessionService, SessionService>();

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);

    // Enable detailed error messages in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
}).AddJsonProtocol(o =>
        o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddStackExchangeRedis(config.GetRedisConnectionString());
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCredentials", cors =>
    {
        cors.WithOrigins("https://*.ttx.gg")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

if (builder.Environment.IsProduction())
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/var/ttx/keys"))
        .SetApplicationName("TTX");


builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "api.ttx.gg",
            ValidAudience = "ttx.gg",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSecretKey()))
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
    app.UseHttpsRedirection();
else
    app.UseDeveloperExceptionPage();

app.UseHttpLogging();
app.MapOpenApi();
app.UseSwagger();
app.MapSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.UseCors("AllowCredentials");
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<TtxExceptionMiddleware>();
app.MapControllers();
app.MapHub<EventHub>("hubs/events");

using var scope = app.Services.CreateScope();
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    if (args.Length > 0 && args[0] == "seed")
    {
        Console.WriteLine("Seeding database...");
        SeedService seed = new(context);
        await seed.Seed();
        Console.WriteLine("Done.");
        return;
    }
}

app.Run();

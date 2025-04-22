using System.Text;
using System.Text.Json.Serialization;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TTX.Api.Interfaces;
using TTX.Api.Middleware;
using TTX.Api.Provider;
using TTX.Api.Services;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Commands.Players.AuthenticateDiscordUser;
using TTX.Commands.Players.OnboardTwitchUser;
using TTX.Infrastructure.Data;
using TTX.Infrastructure.Discord;
using TTX.Infrastructure.Twitch;
using TTX.Interfaces.Discord;
using TTX.Interfaces.Twitch;
using TTX.Queries.Creators.FindCreator;
using TTX.Queries.Creators.IndexCreators;
using TTX.Queries.Creators.PullLatestHistory;
using TTX.Queries.Players.FindPlayer;
using TTX.Queries.Players.IndexPlayers;
[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

builder.Configuration.AddEnvironmentVariables("TTX_");
var config = new ConfigProvider(builder.Configuration);

// Add services to the container.
builder.Services
    .AddLogging(options =>
    {
        options.AddConsole();
        options.AddDebug();
    })
    .AddHttpLogging()
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddSingleton<IConfigProvider>(provider => config)
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
    .AddTransient<ITwitchAuthService, TwitchAuthService>(provider =>
    {
        return new TwitchAuthService(
            clientId: config.GetTwitchClientId(),
            clientSecret: config.GetTwitchClientSecret(),
            redirectUri: config.GetTwitchRedirectUri()
        );
    })
    .AddTransient<IDiscordAuthService, DiscordAuthService>(provider =>
    {
        return new DiscordAuthService(
            clientId: config.GetDiscordClientId(),
            clientSecret: config.GetDiscordClientSecret()
        );
    })
    .AddMediatR(cfg =>
    {
        // creator queries
        cfg.RegisterServicesFromAssemblyContaining<IndexCreatorsHandler>();
        cfg.RegisterServicesFromAssemblyContaining<FindCreatorHandler>();
        cfg.RegisterServicesFromAssemblyContaining<PullLatestHistoryHandler>();
        // player queries
        cfg.RegisterServicesFromAssemblyContaining<FindPlayerHandler>();
        cfg.RegisterServicesFromAssemblyContaining<IndexPlayersHandler>();

        // creator commands
        cfg.RegisterServicesFromAssemblyContaining<OnboardTwitchCreatorHandler>();
        cfg.RegisterServicesFromAssemblyContaining<RecordNetChangeHandler>();
        cfg.RegisterServicesFromAssemblyContaining<UpdateStreamStatusHandler>();
        cfg.RegisterServicesFromAssemblyContaining<OnboardTwitchCreatorHandler>();
        // lootbox commands
        cfg.RegisterServicesFromAssemblyContaining<OpenLootBoxCommand>();
        // ordering commands
        cfg.RegisterServicesFromAssemblyContaining<PlaceOrderHandler>();
        // player commands
        cfg.RegisterServicesFromAssemblyContaining<OnboardTwitchUserHandler>();
        cfg.RegisterServicesFromAssemblyContaining<AuthenticateDiscordUserHandler>();
    })
    .AddTransient<ISessionService, SessionService>();

builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

if (builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/var/ttx/keys"))
        .SetApplicationName("TTX");
}

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
            ValidIssuer = "ttx.gg",
            ValidAudience = "ttx.gg",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSecretKey())),
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpLogging();
app.MapOpenApi();
app.UseSwagger();
app.MapSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<TtxExceptionMiddleware>();
app.MapControllers();

using var scope = app.Services.CreateScope();
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
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

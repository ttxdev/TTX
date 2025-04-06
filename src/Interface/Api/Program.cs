using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using TTX.Infrastructure.Data.Repositories;
using TTX.Core.Services;
using TTX.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using TTX.Core.Repositories;
using TTX.Interface.Api.Provider;
using TTX.Core.Interfaces;
using TTX.Infrastructure.Twitch;
using TTX.Interface.Api.Services;
[assembly: ApiController]


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("TTX_");
var config = new ConfigProvider(builder.Configuration);

builder.Services
    .AddLogging()
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddSingleton<IConfigProvider>(provider => config)
   .AddDbContextPool<ApplicationDbContext>(
        options => options.UseNpgsql(config.GetConnectionString()),
        700
    )
    .AddHttpContextAccessor()
    .AddSwaggerGen(options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();
    })
    .AddTransient<ITwitchService, TwitchService>(provider =>
    {
        var config = provider.GetRequiredService<IConfigProvider>();

        return new TwitchService(
            clientId: config.GetTwitchClientId(),
            clientSecret: config.GetTwitchClientSecret(),
            redirectUri: config.GetTwitchRedirectUri()
        );
    })
    .AddTransient<ISessionService, SessionService>(provider =>
    {
        var config = provider.GetRequiredService<IConfigProvider>();
        return new SessionService(config);
    })
    .AddTransient<IVoteRepository, VoteRepository>()
    .AddTransient<ICreatorRepository, CreatorRepository>()
    .AddTransient<IUserRepository, UserRepository>()
    .AddTransient<IUserService, UserService>()
    .AddTransient<ICreatorService, CreatorService>()
    .AddHttpLogging()
    .AddHttpClient()
    .AddControllers();

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

var app = builder.Build();

app.UseCookiePolicy();
app.MapControllers();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.MapSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAllOrigins");

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await context.Database.MigrateAsync();

app.Run();

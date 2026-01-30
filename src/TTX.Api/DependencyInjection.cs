using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using TTX.Api.Jobs;
using TTX.App.Data;
using SessionOptions = TTX.Api.Options.SessionOptions;

namespace TTX.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddTtxApi(this IServiceCollection services, IWebHostEnvironment env, IConfiguration config)
    {
        services
            .Configure<JsonSerializerOptions>(o => o.Converters.Add(new JsonStringEnumConverter()))
            .AddDistributedMemoryCache()
            .AddSession()
            .AddHttpContextAccessor()
            .AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Services
            // Swagger
            .AddEndpointsApiExplorer()
            .AddOpenApi()
            .AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();
                options.NonNullableReferenceTypesAsRequired();
            })
            // SignalR
            .AddHostedService<EventHubDispatcher>()
            .Configure<HubOptions>(config.GetSection("SignalR"))
            .Configure<WebSocketOptions>(config.GetSection("Websocket"))
            .AddSignalR()
            .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Services
            // Cors
            .AddCors(options =>
            {
                options.AddPolicy("AllowCredentials", cors =>
                {
                    cors.WithOrigins("https://ttx.gg", "http://127.0.0.1:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            })
            // Logging
            .AddHttpLogging()
            .AddLogging(opt =>
            {
                opt.AddConsole();
                if (env.IsDevelopment())
                {
                    opt.AddDebug();
                }
            })
            // Health Checks
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<ApplicationDbContext>()
            .Services
            // Rate Limiting
            .AddRateLimiter(options =>
            {
                options.AddPolicy("TransactionRateLimiter", context =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(10)
                        });
                });

                options.RejectionStatusCode = 429;
            })
            // Authentication
            .Configure<SessionOptions>(config.GetSection("Sessions"))
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer((opt) =>
            {
                IConfiguration sessions = config.GetSection("Sessions");
                SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(sessions.GetValue<string>("Key")!));

                IConfiguration validation = config.GetSection("Validation");
                opt.TokenValidationParameters = new()
                {
                    ValidateIssuer = validation.GetValue<bool>("ValidateIssuer"),
                    ValidateAudience = validation.GetValue<bool>("ValidateAudience"),
                    ValidateLifetime = validation.GetValue<bool>("ValidateLifetime"),
                    ValidateIssuerSigningKey = validation.GetValue<bool>("ValidateIssuerSigningKey"),
                    ValidIssuer = validation.GetValue<string>("ValidIssuer"),
                    ValidAudience = validation.GetValue<string>("ValidAudience"),
                    IssuerSigningKey = key
                };
            });

        if (env.IsProduction())
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/var/ttx/keys"))
                .SetApplicationName("TTX.Api");
        }

        return services;
    }
}

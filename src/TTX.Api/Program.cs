using Microsoft.AspNetCore.Mvc;
using TTX.Api.Hubs;
using TTX.Infrastructure;
using TTX.App;
using TTX.Api;
using TTX.Api.Data.Seed;
using TTX.App.Data;
[assembly: ApiController]

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services
    .ConfigureDbContext<ApplicationDbContext>(opt =>
        {
            opt.UseAsyncSeeding(async (dbContext, seed, cancellationToken) =>
            {
                if (!seed || !builder.Environment.IsDevelopment())
                {
                    return;
                }
                Console.WriteLine("Seeding...");
                await SeedService.Seed((ApplicationDbContext)dbContext, cancellationToken);
                Console.WriteLine("Done.");
            });
        })
    .AddTtx(builder.Configuration.GetSection("TTX:Core"))
    .AddTtxInfra(builder.Configuration.GetSection("TTX:Infrastructure"))
    .AddTtxApi(builder.Environment, builder.Configuration.GetSection("TTX:Api"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTtxJobs(builder.Configuration.GetSection("TTX:Jobs"));
}

WebApplication app = builder.Build();
app.UseHttpLogging();
app.UseSwagger().UseSwaggerUI();
app.MapOpenApi();
app.MapHealthChecks("/health");
app.UseCors("AllowCredentials");
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHub<EventHub>("hubs/events");
app.UseSession();
app.UseWebSockets();
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

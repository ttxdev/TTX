using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Dto.Creators;
using TTX.App.Interfaces.Platforms;
using TTX.App.Services.Creators;
using TTX.App.Services.Creators.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Tests.App.Infrastructure.Platforms;
using TTX.Domain.Platforms;
using TTX.App.Data;

namespace TTX.Tests.App.Services.Creators;

[TestClass]
public class ManageCreatorsTests : ServiceTests
{
    [TestMethod]
    public async Task CreatorCanOnboard()
    {
        const Platform platform = Platform.Twitch;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        PlatformUserService platformUserService = (scope.ServiceProvider.GetRequiredKeyedService<IPlatformUserService>(platform) as PlatformUserService)!;
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlatformUser pUser = _platformUserFactory.Create();
        Ticker ticker = _tickerFactory.Create();
        platformUserService.Inject(pUser);

        await creatorService.Onboard(new OnboardRequest()
        {
            Platform = platform,
            PlatformId = pUser.Id,
            Ticker = ticker
        });

        Creator? creator = await dbContext.Creators.FirstOrDefaultAsync(c => c.Platform == platform && c.PlatformId == pUser.Id, TestContext.CancellationToken);
        Assert.IsNotNull(creator);
        Assert.AreEqual(pUser.Username, creator.Slug);
        Assert.AreEqual(pUser.DisplayName, creator.Name);
        Assert.AreEqual(ticker, creator.Ticker);
    }

    [TestMethod]
    public async Task CreatorCanOptOut()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Creator? creator = _creatorFactory.Create();
        dbContext.Creators.Add(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        Result<CreatorOptOutDto> result = await creatorService.OptOut(creator.Slug);

        creator = await dbContext.Creators.FirstOrDefaultAsync(c => c.Id == creator.Id, TestContext.CancellationToken);
        Assert.IsNull(creator);
        Assert.IsTrue(result.IsSuccessful);
    }

    [TestMethod]
    public async Task CreatorOptOutPreventsOnboard()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        PlatformUserService platformUserService = (scope.ServiceProvider.GetRequiredKeyedService<IPlatformUserService>(Platform.Twitch) as PlatformUserService)!;
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlatformUser pUser = _platformUserFactory.Create();
        Creator? creator = _creatorFactory.Create(platformId: pUser.Id);
        platformUserService.Inject(pUser);
        dbContext.Creators.Add(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);
        await creatorService.OptOut(creator.Slug);

        Result<ModelId> result = await creatorService.Onboard(new OnboardRequest
        {
            Platform = Platform.Twitch,
            PlatformId = pUser.Id,
            Ticker = _tickerFactory.Create(),
            Username = pUser.Username
        });

        Assert.IsInstanceOfType<CreatorOptedOutException>(result.Error);
    }
}

using TTX.Commands.CreatorApplications.UpdateCreatorApplication;
using TTX.Tests.Factories;
using TTX.Models;
using TTX.Exceptions;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class UpdateCreatorApplicationTests : ApplicationTests
{
    [TestMethod]
    public async Task ApproveApplication_ShouldPass()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var application = CreatorApplication.Create(
            name: "Elian",
            submitter: player,
            twitchId: "6969",
            ticker: "ELIANISCOOL"
        );
        DbContext.CreatorApplications.Add(application);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new UpdateCreatorApplicationCommand
        {
            ApplicationId = application.Id,
            Status = CreatorApplicationStatus.Approved
        });

        Assert.AreEqual(CreatorApplicationStatus.Approved, result.Status);
    }

    [TestMethod]
    public async Task ApprovedApplication_ShouldFail()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var application = CreatorApplication.Create(
            name: "Elian",
            submitter: player,
            twitchId: "6969",
            ticker: "ELIANISCOOL"
        );

        DbContext.CreatorApplications.Add(application);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsExceptionAsync<InvalidActionException>(async () =>
        {
            await Sender.Send(new UpdateCreatorApplicationCommand
            {
                ApplicationId = application.Id,
                Status = CreatorApplicationStatus.Approved
            });

            await Sender.Send(new UpdateCreatorApplicationCommand
            {
                ApplicationId = application.Id,
                Status = CreatorApplicationStatus.Approved
            });
        });
    }
}

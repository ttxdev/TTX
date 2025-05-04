using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Notifications.Creators;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;
using TTX.Models;
using TTX.Commands.Creators.UpdateCreatorApplication;
using TTX.Exceptions;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class UpdateCreatorApplicationTests : ApplicationTests
{
    [TestMethod]
    public async Task ApproveApplication_ShouldPass()
    {
        var application = CreatorApplication.Create(
            twitchId: "6969",
            ticker: "ELIANISCOOL"
        );
        DbContext.Applications.Add(application);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new UpdateCreatorApplicationCommand
        {
            ApplicationId = application.Id,
            Status = CreatorApplicationStatus.Approved
        });

        Assert.AreEqual(CreatorApplicationStatus.Approved, result.Status);
    }

    [TestMethod]
    public async Task ApplicationDoesNotExist_ShouldFail()
    {
        await Assert.ThrowsExceptionAsync<CreatorApplicationNotFoundException>(async () =>
        {
            await Sender.Send(new UpdateCreatorApplicationCommand
            {
                ApplicationId = 42069,
                Status = CreatorApplicationStatus.Approved
            });
        });
    }

    [TestMethod]
    public async Task ApprovedApplication_ShouldFail()
    {
        var application = CreatorApplication.Create(
            twitchId: "6969",
            ticker: "ELIANISCOOL"
        );

        DbContext.Applications.Add(application);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsExceptionAsync<CreatorApplicationAlreadyCompletedException>(async () =>
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

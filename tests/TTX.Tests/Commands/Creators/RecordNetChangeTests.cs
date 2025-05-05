using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Notifications.Creators;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class RecordNetChangeTests : ApplicationTests
{
    [TestMethod]
    public async Task RecordNetChange_ShouldRecordProperly()
    {
        var netChange = 50;
        var value = 100;
        var creator = CreatorFactory.Create(value);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new RecordNetChangeCommand
        {
            Username = creator.Slug,
            NetChange = netChange
        });

        Assert.AreEqual(value + netChange, result.Creator.Value);
        Assert.AreEqual(value + netChange, result.Value);
    }

    [TestMethod]
    public async Task RecordNetChange_ShouldNotifyUpdateCreator()
    {
        var netChange = 50;
        var value = 100;
        var creator = CreatorFactory.Create(value);
        var vHandler = ServiceProvider.GetRequiredService<UpdateCreatorValueNotificationHandler>();
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        await Sender.Send(new RecordNetChangeCommand
        {
            Username = creator.Slug,
            NetChange = netChange
        });
        var result = vHandler.FindNotification<UpdateCreatorValue>(v => v.Vote.CreatorId == creator.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(value + netChange, result.Vote.Value);
    }
}
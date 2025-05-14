using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Unicode;
using Microsoft.Extensions.DependencyInjection;
using TTX.Api.Interfaces;
using TTX.Dto.Players;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Api;

[TestClass]
public class Transaction : ApplicationTests
{
    [TestMethod]
    public async Task PlaceOrder_ShouldHitRateLimit()
    {
        //Arrange
        var credits = 1000;
        var quantity = 1;
        var creatorValue = 5;
        var creator = CreatorFactory.Create(creatorValue);
        var player = PlayerFactory.Create(credits);

        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var application = new TTXWebApplicationFactory();
        var sessions = application.Services.GetRequiredService<ISessionService>();
        var token = sessions.CreateSession(PlayerPartialDto.Create(player));
        var client = application.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Act
        var data = JsonContent.Create(new { 
            action = TransactionAction.Buy.ToString(),
            creator = creator.Slug.ToString(),
            amount = quantity 
        });
        bool gotTooManyRequests = false;

        foreach (var item in Enumerable.Range(0, 11))
        {
            var res = await client.PostAsync("/transactions", data);

            if(res.StatusCode == HttpStatusCode.TooManyRequests) {
                gotTooManyRequests = true;
                break;
            }
        }

        Assert.AreEqual(true, gotTooManyRequests);
    }
}

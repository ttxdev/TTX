using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TTX.Api.Interfaces;
using TTX.Dto.Players;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Api
{
    [TestClass]
    public class Transaction : ApplicationTests
    {
        [TestMethod]
        public async Task PlaceOrder_ShouldHitRateLimit()
        {
            long credits = 1000;
            int quantity = 1;
            long creatorValue = 5;
            Creator creator = CreatorFactory.Create(creatorValue);
            Player player = PlayerFactory.Create(credits);

            DbContext.Players.Add(player);
            DbContext.Creators.Add(creator);
            await DbContext.SaveChangesAsync();

            TTXWebApplicationFactory application = new();
            ISessionService sessions = application.Services.GetRequiredService<ISessionService>();
            string token = sessions.CreateSession(PlayerPartialDto.Create(player));
            HttpClient client = application.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            JsonContent data = JsonContent.Create(new {
                action = TransactionAction.Buy.ToString(),
                creator = creator.Slug.ToString(),
                amount = quantity
            });
            bool gotTooManyRequests = false;

            foreach (int _ in Enumerable.Range(0, 11))
            {
                HttpResponseMessage res = await client.PostAsync("/transactions", data);

                if (res.StatusCode == HttpStatusCode.TooManyRequests) {
                    gotTooManyRequests = true;
                    break;
                }
            }

            Assert.AreEqual(true, gotTooManyRequests);
        }
    }
}

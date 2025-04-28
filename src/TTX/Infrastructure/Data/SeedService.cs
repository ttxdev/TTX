using System.Text.Json;
using TTX.Infrastructure.Data.Seed;
using TTX.Models;
using TTX.Properties;

namespace TTX.Infrastructure.Data
{
    public class SeedService(ApplicationDbContext context, Random? random = null)
    {
        private readonly Random Random = random ?? new Random();

        public async Task Seed(CancellationToken ct = default)
        {
            SeedCreators();
            SeedPlayers();
            await context.SaveChangesAsync(ct);
        }

        private List<Creator> SeedCreators()
        {
            List<Creator> creators = [];
            List<PCreatorSeedData>? seed =
                JsonSerializer.Deserialize<List<PCreatorSeedData>>(Resources.TTX_Seed_Creators)!;
            foreach (PCreatorSeedData data in seed)
            {
                Creator creator = Creator.Create(
                    data.DisplayName,
                    data.Login,
                    avatarUrl: new Uri(data.ProfileImageUrl),
                    ticker: data.Ticker,
                    twitchId: data.Id,
                    value: Random.Next(1, 1000)
                );

                creators.Add(creator);
                context.Creators.Add(creator);
            }


            return creators;
        }

        private List<Player> SeedPlayers()
        {
            List<PlayerSeedData>? seed = JsonSerializer.Deserialize<List<PlayerSeedData>>(Resources.TTX_Seed_Players)!;
            List<Player> players = [];
            foreach (PlayerSeedData data in seed)
            {
                Player player = Player.Create(
                    data.Name,
                    data.Slug,
                    avatarUrl: new Uri(data.AvatarUrl),
                    twitchId: data.TwitchId,
                    credits: Random.Next(Player.StarterCredits, 1000)
                );

                players.Add(player);
                context.Players.Add(player);
            }

            return players;
        }
    }
}
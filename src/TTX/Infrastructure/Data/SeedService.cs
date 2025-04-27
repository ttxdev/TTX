using System.Text.Json;
using TTX.Infrastructure.Data.Seed;
using TTX.Models;

namespace TTX.Infrastructure.Data;

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
        var seed = JsonSerializer.Deserialize<List<PCreatorSeedData>>(Properties.Resources.TTX_Seed_Creators)!;
        foreach (var data in seed)
        {
            var creator = Creator.Create(
                name: data.DisplayName,
                slug: data.Login,
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
        var seed = JsonSerializer.Deserialize<List<PlayerSeedData>>(Properties.Resources.TTX_Seed_Players)!;
        List<Player> players = [];
        foreach (var data in seed)
        {
            var player = Player.Create(
                name: data.Name,
                slug: data.Slug,
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

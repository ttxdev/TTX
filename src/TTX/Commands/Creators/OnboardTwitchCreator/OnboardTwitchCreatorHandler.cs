using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.OnboardTwitchCreator;

public class OnboardTwitchCreatorHandler(
    ApplicationDbContext context,
    ITwitchAuthService twitch
) : ICommandHandler<OnboardTwitchCreatorCommand, Creator>
{
    public async Task<Creator> Handle(OnboardTwitchCreatorCommand request, CancellationToken ct = default)
    {
        if (await IsTickerTaken(request.Ticker, ct))
            throw new CreatorTickerTakenException();

        var tUser = await twitch.Find(request.Username) ?? throw new TwitchUserNotFoundException();
        var creator = Creator.Create(
            name: tUser.DisplayName,
            slug: tUser.Login,
            twitchId: tUser.Id,
            avatarUrl: tUser.AvatarUrl,
            ticker: request.Ticker
        );

        context.Creators.Add(creator);
        await context.SaveChangesAsync(ct);

        return creator;
    }

    private async Task<bool> IsTickerTaken(Ticker ticker, CancellationToken ct)
    {
        var exists = await context.Creators
            .AnyAsync(c => c.Ticker == ticker, ct);

        return exists;
    }
}

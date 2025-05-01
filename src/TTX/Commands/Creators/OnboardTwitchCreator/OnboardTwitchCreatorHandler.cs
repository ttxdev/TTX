using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.Notifications.Creators;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.OnboardTwitchCreator
{
    public class OnboardTwitchCreatorHandler(
        IMediator mediatr,
        ApplicationDbContext context,
        ITwitchAuthService twitch
    ) : ICommandHandler<OnboardTwitchCreatorCommand, Creator>
    {
        public async Task<Creator> Handle(OnboardTwitchCreatorCommand request, CancellationToken ct = default)
        {
            if (await IsTickerTaken(request.Ticker, ct))
            {
                throw new CreatorTickerTakenException();
            }

            TwitchUser tUser = await twitch.Find(request.Username) ?? throw new TwitchUserNotFoundException();
            Creator creator = Creator.Create(
                tUser.DisplayName,
                tUser.Login,
                tUser.Id,
                tUser.AvatarUrl,
                request.Ticker
            );

            context.Creators.Add(creator);
            await context.SaveChangesAsync(ct);

            await mediatr.Publish(CreateCreator.Create(creator), ct);

            return creator;
        }

        private async Task<bool> IsTickerTaken(Ticker ticker, CancellationToken ct)
        {
            bool exists = await context.Creators
                .AnyAsync(c => c.Ticker == ticker, ct);

            return exists;
        }
    }
}
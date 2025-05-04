using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.Notifications.Creators;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.CreatorApply
{
    public class CreatorApplyHandler(
        IMediator mediatr,
        ApplicationDbContext context,
        ITwitchAuthService twitch
    ) : ICommandHandler<CreatorApplyCommand, CreatorApplication>
    {
        public async Task<CreatorApplication> Handle(CreatorApplyCommand request, CancellationToken ct = default)
        {
            if (await IsTickerTaken(request.Ticker, ct))
            {
                throw new CreatorTickerTakenException();
            }

            if (await CreatorExists(request.Username, ct))
            {
                throw new CreatorExistsException();
            }

            TwitchUser tUser = await twitch.Find(request.Username) ?? throw new TwitchUserNotFoundException();
            CreatorApplication creatorApplication = CreatorApplication.Create(
                submitterId: request.SubmitterId,
                ticker: request.Ticker,
                twitchId: tUser.Id
            );

            context.Applications.Add(creatorApplication);
            await context.SaveChangesAsync(ct);

            //await mediatr.Publish(CreateCreator.Create(creator), ct);

            return creatorApplication;
        }

        private async Task<bool> IsTickerTaken(Ticker ticker, CancellationToken ct)
        {
            bool exists = await context.Creators
                .AnyAsync(c => c.Ticker == ticker, ct);

            return exists;
        }

        private async Task<bool> CreatorExists(Slug username, CancellationToken ct)
        {
            bool exists = await context.Creators
                .AnyAsync(c => c.Slug == username, ct);

            return exists;
        }
    }
}

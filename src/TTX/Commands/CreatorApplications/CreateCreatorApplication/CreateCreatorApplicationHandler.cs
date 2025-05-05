using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.CreatorApplications.CreateCreatorApplication
{
    public class CreateCreatorApplicationHandler(
        IMediator mediatr,
        ApplicationDbContext context,
        ITwitchAuthService twitch
    ) : ICommandHandler<CreateCreatorApplicationCommand, CreatorApplication>
    {
        public async Task<CreatorApplication> Handle(CreateCreatorApplicationCommand request,
            CancellationToken ct = default)
        {
            if (await IsTickerTaken(request.Ticker, ct))
            {
                throw new InvalidActionException("Ticker already taken");
            }

            if (await CreatorExists(request.Username, ct))
            {
                throw new InvalidActionException("Creator already exists");
            }

            Player player = await context.Players.SingleOrDefaultAsync(p => p.Id == request.SubmitterId, ct) ??
                            throw new NotFoundException<Player>();
            TwitchUser tUser = await twitch.Find(request.Username) ?? throw new NotFoundException<TwitchUser>();
            CreatorApplication app = CreatorApplication.Create(
                tUser.DisplayName,
                submitter: player,
                ticker: request.Ticker,
                twitchId: tUser.Id
            );

            context.CreatorApplications.Add(app);
            await context.SaveChangesAsync(ct);

            await mediatr.Publish(
                Notifications.CreatorApplications.CreateCreatorApplication.Create(app),
                ct
            );

            return app;
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
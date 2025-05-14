using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Dto.Creators;
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
    ) : ICommandHandler<OnboardTwitchCreatorCommand, CreatorDto>
    {
        public async Task<CreatorDto> Handle(OnboardTwitchCreatorCommand request, CancellationToken ct = default)
        {
            if (await IsTickerTaken(request.Ticker, ct))
            {
                throw new InvalidActionException("Ticker is already taken");
            }

            TwitchUser tUser = await FindTwitchUser(request);
            Creator? creator = await CreatorExists(tUser.Id, ct);
            if (creator is not null)
            {
                if (creator.Sync(tUser.DisplayName, tUser.Login, tUser.AvatarUrl))
                {
                    context.Creators.Update(creator);
                    await context.SaveChangesAsync(ct);
                }

                return CreatorDto.Create(creator);
            }

            if (await IsOptedOut(tUser.Id, ct))
            {
                throw new InvalidActionException("Creator is opted out");
            }

            creator = Creator.Create(
                tUser.DisplayName,
                tUser.Login,
                tUser.Id,
                tUser.AvatarUrl,
                request.Ticker
            );

            context.Creators.Add(creator);
            await context.SaveChangesAsync(ct);
            await mediatr.Publish(CreateCreator.Create(creator), ct);

            return CreatorDto.Create(creator);
        }

        private async Task<bool> IsTickerTaken(Ticker ticker, CancellationToken ct)
        {
            bool exists = await context.Creators
                .AnyAsync(c => c.Ticker == ticker, ct);

            return exists;
        }

        private async Task<TwitchUser> FindTwitchUser(OnboardTwitchCreatorCommand request)
        {
            TwitchUser? tUser;
            if (request.Username is not null)
            {
                tUser = await twitch.Find(request.Username);
            }
            else if (request.TwitchId is not null)
            {
                tUser = await twitch.FindById(request.TwitchId);
            }
            else
            {
                throw new InvalidOperationException("Username or TwitchId must be provided");
            }

            return tUser ?? throw new NotFoundException<TwitchUser>();
        }

        private Task<Creator?> CreatorExists(TwitchId twitchId, CancellationToken ct)
        {
            return context.Creators.SingleOrDefaultAsync(c => c.TwitchId == twitchId, ct);
        }

        private Task<bool> IsOptedOut(TwitchId twitchId, CancellationToken ct)
        {
            return context.CreatorOptOuts.AnyAsync(c => c.TwitchId == twitchId, ct);
        }
    }
}

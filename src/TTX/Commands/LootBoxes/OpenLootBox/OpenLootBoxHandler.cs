using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public class OpenLootBoxHandler(
        ApplicationDbContext context,
        IMediator mediator,
        Random? random = null
    ) : ICommandHandler<OpenLootBoxCommand, OpenLootBoxResult>
    {
        public const int MinValue = 100;
        public readonly Random Random = random ?? new Random();

        public async Task<OpenLootBoxResult> Handle(OpenLootBoxCommand request, CancellationToken ct = default)
        {
            Player player = await context.Players
                                .Include(p => p.LootBoxes)
                                .SingleOrDefaultAsync(p => p.Id == request.ActorId, ct)
                            ?? throw new PlayerNotFoundException();
            LootBox lootBox = player.LootBoxes.SingleOrDefault(l => l.Id == request.LootBoxId) ??
                              throw new LootBoxNotFoundException();
            OpenLootBoxResult result = lootBox.Open(
                await context.Creators.Where(c => c.Value >= MinValue).ToArrayAsync(ct), 
                Random);

            await mediator.Publish(new Notifications.LootBoxes.OpenLootBox(result), ct);

            return result;
        }
    }
}
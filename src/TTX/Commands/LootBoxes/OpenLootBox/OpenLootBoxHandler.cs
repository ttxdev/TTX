using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Dto.LootBoxes;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.Notifications.Transactions;
using TTX.ValueObjects;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public class OpenLootBoxHandler(
        ApplicationDbContext context,
        IMediator mediator,
        Random? random = null
    ) : ICommandHandler<OpenLootBoxCommand, LootBoxResultDto>
    {
        public const int MinValue = 100;
        private readonly Random Random = random ?? new Random();

        public async Task<LootBoxResultDto> Handle(OpenLootBoxCommand request, CancellationToken ct = default)
        {
            Player player = await context.Players
                                .Include(p => p.LootBoxes)
                                .SingleOrDefaultAsync(p => p.Id == request.ActorId, ct)
                            ?? throw new NotFoundException<Player>();
            LootBox lootBox = player.LootBoxes.SingleOrDefault(l => l.Id == request.LootBoxId) ??
                              throw new NotFoundException<LootBox>();
            OpenLootBoxResult result = lootBox.Open(
                await context.Creators.Where(c => c.Value >= MinValue).ToArrayAsync(ct),
                Random);
            Transaction tx = player.Give(lootBox.Result!);

            context.Transactions.Add(tx);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(CreateTransaction.Create(tx), ct);
            await mediator.Publish(Notifications.LootBoxes.OpenLootBox.Create(result), ct);

            return LootBoxResultDto.Create(result);
        }
    }
}
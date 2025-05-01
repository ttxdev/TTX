using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.Notifications.Creators;

namespace TTX.Commands.Creators.RecordNetChange
{
    public class RecordNetChangeHandler(ApplicationDbContext context, IMediator mediator)
        : ICommandHandler<RecordNetChangeCommand, Vote>
    {
        public async Task<Vote> Handle(RecordNetChangeCommand request, CancellationToken ct = default)
        {
            Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.CreatorSlug, ct)
                              ?? throw new CreatorNotFoundException();
            Vote vote = creator.ApplyNetChange(request.NetChange);

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId.Value}, {vote.Value.Value}, {vote.Time})",
                ct);
            context.Creators.Update(creator);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(new UpdateCreatorValue(vote), ct);

            return vote;
        }
    }
}
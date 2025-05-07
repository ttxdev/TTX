using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Dto.Creators;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.Notifications.Creators;

namespace TTX.Commands.Creators.RecordNetChange
{
    public class RecordNetChangeHandler(ApplicationDbContext context, IMediator mediator)
        : ICommandHandler<RecordNetChangeCommand, VoteDto>
    {
        public async Task<VoteDto> Handle(RecordNetChangeCommand request, CancellationToken ct = default)
        {
            Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.Username, ct)
                              ?? throw new NotFoundException<Creator>();
            Vote vote = creator.ApplyNetChange(request.NetChange);

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId.Value}, {vote.Value.Value}, {vote.Time})",
                ct);
            context.Creators.Update(creator);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(UpdateCreatorValue.Create(vote), ct);

            return VoteDto.Create(vote);
        }
    }
}
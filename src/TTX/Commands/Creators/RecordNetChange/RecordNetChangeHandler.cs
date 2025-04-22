using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Commands.Creators.RecordNetChange;

public class RecordNetChangeHandler(ApplicationDbContext context) : ICommandHandler<RecordNetChangeCommand, Vote>
{
    public async Task<Vote> Handle(RecordNetChangeCommand request, CancellationToken ct = default)
    {
        var creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.CreatorSlug, ct)
            ?? throw new CreatorNotFoundException();
        var vote = creator.ApplyNetChange(request.NetChange);

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId.Value}, {vote.Value.Value}, {vote.Time})", cancellationToken: ct);
        context.Creators.Update(creator);
        await context.SaveChangesAsync(ct);

        return vote;
    }
}

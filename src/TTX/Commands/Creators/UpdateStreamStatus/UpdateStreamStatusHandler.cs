using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using MediatR;

namespace TTX.Commands.Creators.UpdateStreamStatus;

public class UpdateStreamStatusHandler(ApplicationDbContext context, IMediator mediator) : ICommandHandler<UpdateStreamStatusCommand, StreamStatus>
{
    public async Task<StreamStatus> Handle(UpdateStreamStatusCommand request, CancellationToken ct = default)
    {
        Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.CreatorSlug, ct)
                          ?? throw new CreatorNotFoundException();

        if (request.IsLive)
        {
            creator.StreamStatus.Started(request.At);
        }
        else
        {
            creator.StreamStatus.Ended(request.At);
        }

        context.Update(creator);
        await context.SaveChangesAsync(ct);
        await mediator.Publish(new Notifications.Creators.UpdateStreamStatus
        {
            StreamStatus = creator.StreamStatus,
        }, ct);

        return creator.StreamStatus;
    }
}

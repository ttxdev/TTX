using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Commands.Creators.UpdateStreamStatus
{
    public class UpdateStreamStatusHandler(ApplicationDbContext context, IMediator mediator)
        : ICommandHandler<UpdateStreamStatusCommand, StreamStatus>
    {
        public async Task<StreamStatus> Handle(UpdateStreamStatusCommand request, CancellationToken ct = default)
        {
            Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.Username, ct)
                              ?? throw new NotFoundException<Creator>();

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
            await mediator.Publish(Notifications.Creators.UpdateStreamStatus.Create(creator), ct);

            return creator.StreamStatus;
        }
    }
}
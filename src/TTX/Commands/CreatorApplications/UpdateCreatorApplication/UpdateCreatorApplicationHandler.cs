using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Commands.CreatorApplications.UpdateCreatorApplication
{
    public class UpdateCreatorApplicationHandler(
        IMediator mediatr,
        ApplicationDbContext context
    ) : ICommandHandler<UpdateCreatorApplicationCommand, CreatorApplication>
    {
        public async Task<CreatorApplication> Handle(UpdateCreatorApplicationCommand request,
            CancellationToken ct = default)
        {
            CreatorApplication application = await context.CreatorApplications
                                                 .SingleOrDefaultAsync(a => a.Id == request.ApplicationId, ct) ??
                                             throw new NotFoundException<CreatorApplication>();

            application.UpdateStatus(request.Status);
            context.CreatorApplications.Update(application);
            await context.SaveChangesAsync(ct);

            await mediatr.Publish(
                Notifications.CreatorApplications.UpdateCreatorApplication.Create(application),
                ct
            );

            return application;
        }
    }
}
using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.Notifications.Creators;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.UpdateCreatorApplication
{
    public class UpdateCreatorApplicationHandler(
        IMediator mediatr,
        ApplicationDbContext context,
        ITwitchAuthService twitch
    ) : ICommandHandler<UpdateCreatorApplicationCommand, CreatorApplication>
    {
        public async Task<CreatorApplication> Handle(UpdateCreatorApplicationCommand request, CancellationToken ct = default)
        {
          var application = await context.Applications
                .SingleOrDefaultAsync(a => a.Id == request.ApplicationId, ct) ??
                throw new CreatorApplicationNotFoundException();

          application.UpdateStatus(request.Status);
          context.Applications.Update(application);
          await context.SaveChangesAsync(ct);

          return application;
        }
    }
}

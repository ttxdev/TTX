using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.CreatorOptOuts
{
    public class CreatorOptOutHandler(
        ApplicationDbContext context
    ) : ICommandHandler<CreatorOptOutCommand, CreatorOptOut>
    {
        public async Task<CreatorOptOut> Handle(CreatorOptOutCommand request, CancellationToken ct = default)
        {
            Creator creator = await FindCreator(request.Username, ct) ??
                throw new NotFoundException<Creator>();

            CreatorOptOut opt = CreatorOptOut.Create(creator);
            context.CreatorOptOuts.Add(opt);
            context.Creators.Remove(creator);
            await context.SaveChangesAsync(ct);

            return opt;
        }

        private Task<Creator?> FindCreator(Slug username, CancellationToken ct)
        {
            return context.Creators.SingleOrDefaultAsync(c => c.Slug == username, ct);
        }
    }
}
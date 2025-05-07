using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.ValueObjects;
using TTX.Dto.Creators;

namespace TTX.Commands.Creators.CreatorOptOuts
{
    public class CreatorOptOutHandler(
        ApplicationDbContext context
    ) : ICommandHandler<CreatorOptOutCommand, CreatorOptOutDto>
    {
        public async Task<CreatorOptOutDto> Handle(CreatorOptOutCommand request, CancellationToken ct = default)
        {
            Creator creator = await FindCreator(request.Username, ct) ??
                throw new NotFoundException<Creator>();

            CreatorOptOut opt = CreatorOptOut.Create(creator);
            context.CreatorOptOuts.Add(opt);
            context.Creators.Remove(creator);
            await context.SaveChangesAsync(ct);

            return CreatorOptOutDto.Create(opt);
        }

        private Task<Creator?> FindCreator(Slug username, CancellationToken ct)
        {
            return context.Creators.SingleOrDefaultAsync(c => c.Slug == username, ct);
        }
    }
}

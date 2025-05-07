using Microsoft.EntityFrameworkCore;
using TTX.Dto.CreatorApplications;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.CreatorApplications.FindCreatorApplication
{
    public class FindCreatorApplicationHandler(ApplicationDbContext context)
        : IQueryHandler<FindCreatorApplicationQuery, CreatorApplicationDto?>
    {
        public async Task<CreatorApplicationDto?> Handle(FindCreatorApplicationQuery request,
            CancellationToken cancellationToken)
        {
            CreatorApplication? app =
                await context.CreatorApplications.SingleOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            return app is null ? null : CreatorApplicationDto.Create(app);
        }
    }
}
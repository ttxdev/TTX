using MediatR;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.CreatorOptOuts
{
    public class CreatorOptOutCommand : IRequest<CreatorOptOut>
    {
        public required Slug Username { get; init; }
    }
}

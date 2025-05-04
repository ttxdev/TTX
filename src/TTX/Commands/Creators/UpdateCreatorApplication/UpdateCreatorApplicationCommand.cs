using MediatR;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.UpdateCreatorApplication
{
    public readonly struct UpdateCreatorApplicationCommand : IRequest<CreatorApplication>
    {
        public required ModelId ApplicationId { get; init; }
        public required CreatorApplicationStatus Status { get; init; }
    }
}

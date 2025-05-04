using MediatR;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.CreatorApply
{
    public readonly struct CreatorApplyCommand : IRequest<CreatorApplication>
    {
        public required Slug Username { get; init; }
        public required Ticker Ticker { get; init; }
    }
}

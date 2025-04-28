using MediatR;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.OnboardTwitchCreator
{
    public readonly struct OnboardTwitchCreatorCommand : IRequest<Creator>
    {
        public required Slug Username { get; init; }
        public required Ticker Ticker { get; init; }
    }
}
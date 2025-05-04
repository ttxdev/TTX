using MediatR;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.OnboardTwitchCreator
{
    public class OnboardTwitchCreatorCommand : IRequest<Creator>
    {
        public Slug? Username { get; init; }
        public TwitchId? TwitchId { get; init; }
        public required Ticker Ticker { get; init; }
    }
}

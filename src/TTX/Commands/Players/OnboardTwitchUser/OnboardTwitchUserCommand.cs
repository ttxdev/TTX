using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Players.OnboardTwitchUser
{
    public class OnboardTwitchUserCommand : ICommand<Player>
    {
        public required TwitchId Id { get; init; }
    }
}
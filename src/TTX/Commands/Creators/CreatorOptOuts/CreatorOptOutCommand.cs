using System.Text.Json.Serialization;
using MediatR;
using TTX.Models;

namespace TTX.Commands.Creators.CreatorOptOuts
{
    public class CreatorOptOutCommand : IRequest<CreatorOptOut>
    {
        [JsonPropertyName("username")]
        public required string Username { get; init; }
    }
}

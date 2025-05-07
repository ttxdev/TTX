using System.Text.Json.Serialization;
using MediatR;
using TTX.Dto.Creators;

namespace TTX.Commands.Creators.CreatorOptOuts
{
    public class CreatorOptOutCommand : IRequest<CreatorOptOutDto>
    {
        [JsonPropertyName("username")]
        public required string Username { get; init; }
    }
}

using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.RecordNetChange
{
    public readonly struct RecordNetChangeCommand : ICommand<Vote>
    {
        public required Slug CreatorSlug { get; init; }
        public required int NetChange { get; init; }
    }
}
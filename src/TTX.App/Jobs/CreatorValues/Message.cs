using TTX.Domain.ValueObjects;

namespace TTX.App.Jobs.CreatorValues;

public record MessageEvent(ModelId CreatorId, string Content);

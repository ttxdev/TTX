using TTX.Domain.ValueObjects;

namespace TTX.App.Jobs.CreatorValues;

public record NetChangeEvent(ModelId CreatorId, double NetChange);

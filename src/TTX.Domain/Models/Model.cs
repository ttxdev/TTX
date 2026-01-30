using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class Model
{
    public virtual ModelId Id { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void Bump()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

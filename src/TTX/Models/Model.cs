using TTX.ValueObjects;

namespace TTX.Models
{
    public class Model
    {
        public ModelId Id { get; init; } = null!;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public void Bump()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
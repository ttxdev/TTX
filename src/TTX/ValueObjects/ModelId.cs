namespace TTX.ValueObjects;

public class ModelId : ValueObject<int>
{
    public static ModelId Create(int value) => new() { Value = value };
    public static implicit operator ModelId(int value) => Create(value);
    public static implicit operator int(ModelId modelId) => modelId.Value;
}

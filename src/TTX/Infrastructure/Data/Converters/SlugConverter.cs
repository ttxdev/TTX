using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters;

public class SlugConverter() : ValueObjectConverter<Slug, string>(Slug.Create);

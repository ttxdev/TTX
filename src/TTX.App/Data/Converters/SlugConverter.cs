using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class SlugConverter() : ValueObjectConverter<Slug, string>(Slug.Create);

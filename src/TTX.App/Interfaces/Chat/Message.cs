using TTX.Domain.ValueObjects;

namespace TTX.App.Interfaces.Chat;

public record Message(Slug Slug, string Content);

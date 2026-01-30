using Microsoft.IdentityModel.Tokens;

namespace TTX.Api.Options;

public class SessionOptions
{
    public required string Key { get; init; }
    public required TimeSpan Expires { get; init; }
    public required TokenValidationParameters Validation { get; init; }
}

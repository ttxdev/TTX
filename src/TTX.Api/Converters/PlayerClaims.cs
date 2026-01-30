using System.Security.Claims;
using TTX.App.Dto.Players;

namespace TTX.Api.Converters;

public static class PlayerClaims
{
    public static Claim[] ToClaims(this PlayerPartialDto player)
    {
        return [
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
            new Claim(ClaimTypes.GivenName, player.Name),
            new Claim(ClaimTypes.Name, player.Slug),
            new Claim("AvatarUrl", player.AvatarUrl),
            new Claim(ClaimTypes.Role, player.Type.ToString()),
            new Claim("UpdatedAt", player.UpdatedAt.ToString("o"))
        ];
    }
}

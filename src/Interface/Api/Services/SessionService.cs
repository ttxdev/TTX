using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Interface.Api.Provider;

namespace TTX.Interface.Api.Services;

public class SessionService(IConfigProvider config) : ISessionService
{
    public User? CurrentUser { get; set; }

    public bool IsAdmin()
    {
        if (CurrentUser is null)
            return false;

        return CurrentUser.IsAdmin();
    }

    public string CreateSession(User user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim("AvatarUrl", user.AvatarUrl),
        new Claim(ClaimTypes.Role, user.Type.ToString()),
        new Claim("UpdatedAt", user.UpdatedAt.ToString("o"))
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSecretKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
          issuer: "ttx.gg",
          audience: "ttx.gg",
          claims: claims,
          expires: DateTime.Now.AddDays(7),
          signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
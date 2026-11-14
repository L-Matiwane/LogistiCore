using System.Security.Claims;
using System.Text;
using LogistiCore.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LogistiCore.Api.Auth;

public class TokenService
{
    public const string Issuer = "LogistiCore";
    public const string Audience = "LogistiCore.Client";

    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        var secret = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing. Set it with dotnet user-secrets.");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public string CreateToken(User user)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.Name)
            }),
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
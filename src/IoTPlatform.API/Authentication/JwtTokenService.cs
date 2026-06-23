using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IoTPlatform.Models.Entities.Tenancy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IoTPlatform.API.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(User user)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(AppClaimTypes.CompanyId, user.CompanyId.ToString()),
            new(AppClaimTypes.IsSuperAdmin, user.IsSuperAdmin ? "true" : "false"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Courtyard.api.Models;
using Courtyard.api.Options;

namespace Courtyard.api.Services;

public class TokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> options)
    {
        _jwt = options.Value;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("email", user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public DateTime RefreshTokenExpiry() =>
        DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
}
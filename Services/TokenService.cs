using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Courtyard.api.Options;
using Courtyard.api.Models;

namespace Courtyard.api.Services;

public class TokenService
{
	private readonly JwtOptions _jwt

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

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.key));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expire: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
				signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}


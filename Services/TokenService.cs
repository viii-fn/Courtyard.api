using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Courtyard.api.Models;

namespace Courtyard.api.Services;

public class TokenService
{
	private readonly IConfiguration _config;

	public TokenService(IConfiguration _config)
	{
		_config = config;
	}

	public string CreateToken(User user)
	{
		var claims = new List<Claim>
		{
			new Claim("userId", user.Id.ToString()),
			new Claim("email", user.Email)
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expire: DateTime.UtcNow.AddMinutes(60),
				signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}


using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Courtyard.api.Data;
using Courtyard.api.Models;
using Courtyard.api.Services;

namespace Courtyard.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly TokenService _tokens;

	public AuthController(AppDbContext db, TokenService tokens)
	{
		_db = db;
		_tokens = tokens;
	}

	[HttpPost("signup")]
	public async Task<IActionResult> Register(AuthRequest req)
	{
		var email = req.Email.Trim().ToLowerInvariant();
		var exists = await _db.Users.AnyAsync(u => u.Email == email);
		if (exists) return Conflict(new { message = "Email already exists" });

		var user = new User
		{
			Email = email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();

		return Ok(new { token = _tokens.CreateToken(user) });
	}
}

[HttpPost("login")]
public asybc


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

	public AuthController(AppDbContext db, TokenService tokens, IPasswordService passwords)
	{
		_db = db;
		_tokens = tokens;
		_passwords = passwords;
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
			PasswordHash = passwords.Hash(req.Password)
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();

		return Ok(new { token = _tokens.CreateToken(user) });
	}
}

[HttpPost("login")]
public async Task<IActionResult> Login(AuthRequest req)
{
	var email = req.Email.Trim().ToLowerInvariant();

	var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

	if (user is null || !_passwords.Verify(req.Password, user.PasswordHash)) return Unauthorized(new { token = _tokens.CreateToken(user) });

	[Authorize]
	[HttpGet("me")]
	public IActionResult Me()
	{
		var userId = User.FindFirst("userId")?.Value;
		var email = User.FindFirst("email")?.Value;
		return Ok(new { userId, email });
	}
}


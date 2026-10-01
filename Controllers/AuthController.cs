using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Courtyard.api.Data;
using Courtyard.api.Models;
using Courtyard.api.Services;
using Courtyard.api.Dtos;

namespace Courtyard.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly TokenService _tokens;
	private readonly IPasswordService _passwords;

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
			PasswordHash = _passwords.Hash(req.Password)
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();

		return Ok(await IssueTokensAsync(user));
	}

	private async Task<AuthResponse> IssueTokensAsync(User user)
	{
		var rawRefresh = _tokens.GenerateRefreshToken();

		_db.RefreshTokens.Add(new RefreshToken
		{
			UserId = user.Id,
			TokenHash = TokenService.Hash(rawRefresh),
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = _tokens.RefreshTokenExpiry()
		});

		await _db.SaveChangesAsync();
		return new AuthResponse(_tokens.CreateToken(user), rawRefresh);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(AuthRequest req)
	{
		var email = req.Email.Trim().ToLowerInvariant();

		var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

		if (user is null || !_passwords.Verify(req.Password, user.PasswordHash))
		{
			return Unauthorized(new { message = "Invalid Email or Password." });
		}

		return Ok(await IssueTokensAsync(user));
	}

	[Authorize]
	[HttpGet("me")]
	public IActionResult Me()
	{
		var userId = User.FindFirst("userId")?.Value;
		var email = User.FindFirst("email")?.Value;
		return Ok(new { userId, email })
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh(RefreshRequest req)
	{
		var hash = TokenService.Hash(req.RefreshToken);

		var stored = await _db.RefreshTokens
			.Include(t => t.User)
			.FirstOrDefaultAsync(t => t.TokenHash == hash);

		var invalid = Unauthorized(new { message = "Invalid refresh token." });
		if (stored is null) return invalid;

		if (stored.RevokedAt is not null)
		{
			var active = await _db.RefreshTokens
				.Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
				.ToListAsync();

			foreach (var t in active) t.RevokedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync();
			return invalid;
		}

		if (DateTime.UtcNow >= stored.ExpiresAt) return invalid;

		stored.RevokedAt = DateTime.UtcNow;
		return Ok(await IssueTokensAsync(stored.User));
	}

	[HttpPost("logout")]
	public async Task<IActionResult> Logout(RefreshRequest req)
	{
		var hash = TokenService.Hash(req.RefreshToken);
		var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
		
		if (stored is not null && stored.RevokedAt is null)
		{
			stored.RevokedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync();
		}

		return NoContent();
	}
}


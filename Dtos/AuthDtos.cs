using System.ComponentModel.DataAnnotations;

namespace Courtyard.api.Dtos;

public class AuthRequest
{
	[Required, EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required, MinLength(8)]
	public string Password { get; set; } = string.Empty;
}


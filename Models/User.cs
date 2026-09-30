using System.ComponentModel.DataAnnotations;

namespace Courtyard.api.Models;

public class User
{
	public int Id { get; set; }
	[EmailAddress]
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
}
	


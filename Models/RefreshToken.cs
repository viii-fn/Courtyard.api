namespace Courtyard.api.Models;

public class RefreshToken
{
	public int Id { get; set; }
	public string TokenHash { get; set; } = string.Empty;
	public int UserId { get; set; }
	public User user { get; set; } = null;
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public DateTime? RevokedAt { get; set; }
}


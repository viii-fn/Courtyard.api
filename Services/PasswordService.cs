using BCryptNet = BCrypt.Net.BCrypt;

namespace Courtyard.api.Services;

public class PasswordService : IPasswordService
{
	private const int WorkFactor = 12;

	public string Hash(string password) =>
		BCryptNet.HashPassword(password, WorkFactor);

	public bool Verify(string password, string hash) =>
		BCryptNet.Verify(password, hash);
}

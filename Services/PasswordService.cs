using BCryptNet = BCrypt.Net.BCrypt;

namespace Courtyard.api.Services;

public interface IPasswordService
{
	private const int WorkFactor = 12;

	public string Hash(string password) =>
		BCryptNet.HashPassword(password, WorkFactor);

	public bool Verify(string password, string hash) =>
		BCrypt.Verify(password, hash);
}

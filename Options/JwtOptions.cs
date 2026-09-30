using System.ComponentModel.DataAnnotations;

namespace Courtyard.api.Options;

public class JwtOptions
{
	public const string SectionName = "Jwt";

	[Required, MinLength(64)]
	public string Key { get; init; } = string.Empty;

	[Required] public string Issuer { get; init; } = string.Empty;
	[Required] public string Audience { get; init; } = string.Empty;

	[Range(1, 60)]
	public int AccessTokenMinutes { get; init; } = 15;
}

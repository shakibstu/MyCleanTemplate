using System.ComponentModel.DataAnnotations;

namespace MyCleanTemplate.API.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(1)]
    public string Issuer { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    public int ClockSkewSeconds { get; init; } = 30;
}

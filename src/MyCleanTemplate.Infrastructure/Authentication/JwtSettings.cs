using System.ComponentModel.DataAnnotations;

namespace MyCleanTemplate.Infrastructure.Authentication;

/// <summary>
/// Configuration settings for JWT authentication. Validated at startup.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Token issuer.
    /// </summary>
    [Required, MinLength(1)]
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Token audience.
    /// </summary>
    [Required, MinLength(1)]
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Secret key for signing tokens (HMAC-SHA256 needs at least 32 bytes).
    /// </summary>
    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Allowed clock difference when validating token lifetime.
    /// </summary>
    [Range(0, 300)]
    public int ClockSkewSeconds { get; init; } = 30;

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int TokenExpirationMinutes { get; init; } = 60;
}

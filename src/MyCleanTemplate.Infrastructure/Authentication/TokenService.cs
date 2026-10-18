using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MyCleanTemplate.Infrastructure.Authentication;

/// <summary>
/// Service for generating JWT tokens for testing and authentication purposes.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token with the specified claims.
    /// </summary>
    /// <param name="userId">The user ID to include in the token.</param>
    /// <param name="userName">The username to include in the token.</param>
    /// <param name="email">The user email to include in the token.</param>
    /// <param name="isMfaVerified">Whether the user passed an MFA check to obtain this token.</param>
    /// <param name="roles"></param>
    /// <param name="permissions">The list of permissions to include as claims.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(string userId, string userName, string email, bool isMfaVerified, IEnumerable<string> roles,
        IEnumerable<string> permissions);
}

/// <summary>
/// Implementation of <see cref="ITokenService"/> for JWT token generation.
/// </summary>
public class TokenService(IOptions<JwtSettings> options) : ITokenService
{
    /// <inheritdoc />
    public string GenerateToken(string userId, string userName, string email, bool isMfaVerified, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var jwtSettings = options.Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Name, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(AppClaim.MfaVerified, isMfaVerified ? "true" : "false")
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(AppClaim.Permission, permission)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.TokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

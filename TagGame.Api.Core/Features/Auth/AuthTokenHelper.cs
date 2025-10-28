using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Features.Auth;

public static class AuthTokenHelper
{
    public static (string token, DateTimeOffset expires) CreateJwtToken(User user, DateTimeOffset now, JwtOptions jwtOptions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Exp, now.AddMinutes(jwtOptions.AccessMinutes).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
            claims.Add(new Claim("nickname", user.DisplayName));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = now.AddMinutes(jwtOptions.AccessMinutes);

        var jwtToken = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(jwtToken),
            expires);
    }

    public static (string token, string hash, DateTimeOffset expiresAt, Guid familyId) CreateRefreshToken(
        Guid? existingFamilyId,
        DateTimeOffset now,
        JwtOptions options)
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);

        var raw = Base64Url(bytes);
        var hash = HashRefreshToken(raw);
        var expiresAt = now.AddDays(options.RefreshDays);
        var familyId = existingFamilyId ?? Guid.NewGuid();

        return (raw, hash, expiresAt, familyId);
    }

    public static void EnsureToken(RefreshToken? existing, DateTimeOffset now)
    {
        if (existing is null)
            throw new InvalidOperationException("Invalid refresh token.");

        if (existing.RevokedAt.HasValue)
            throw new InvalidOperationException("Refresh token has been revoked.");

        if (existing.ExpiresAt <= now)
            throw new InvalidOperationException("Refresh token has expired.");
    }

    public static string HashRefreshToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string Base64Url(byte[] bytes) => Base64UrlEncoder.Encode(bytes);
}

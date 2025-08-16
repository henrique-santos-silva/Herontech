namespace Herontech.Application.Security;

using System.IdentityModel.Tokens.Jwt;
using Contracts.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Domain;

public sealed class TokenService(IConfiguration cfg) : ITokenService
{
    public AccessTokenResult CreateAccessToken(User u, DateTimeOffset now)
    {
        string issuer   = cfg["Jwt:Issuer"]!;
        string audience = cfg["Jwt:Audience"]!;
        string key      = cfg["Jwt:Key"]!;
        int minutes  = int.Parse(cfg["Jwt:AccessTokenMinutes"] ?? "30");

        SigningCredentials creds = new (
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            algorithm: SecurityAlgorithms.HmacSha256
        );
        DateTimeOffset expiresAt = now.AddMinutes(minutes);

        Claim[] claims =
        [
            new (JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, u.Email),
            new (ClaimTypes.Role,((int) u.Role).ToString()),
            new ("active", u.IsActive.ToString().ToLowerInvariant()),
            new ("email_verified", u.IsEmailConfirmed.ToString().ToLowerInvariant()),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        ];
        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires:  expiresAt.UtcDateTime,
            signingCredentials: creds
        );

        return new AccessTokenResult(
            Token:new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt); 
    }
}

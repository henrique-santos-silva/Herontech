using Herontech.Contracts.Interfaces;
using Herontech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Herontech.Application.Security;

// Application/Security/RefreshTokenService.cs
using Herontech.Domain;

public sealed class RefreshTokenService(AppDbContext db, IConfiguration cfg) : IRefreshTokenService
{
    private TimeSpan Lifetime => TimeSpan.FromDays(int.Parse(cfg["Jwt:RefreshDays"] ?? "7"));

    public async Task<(RefreshToken entity, string rawToken)> IssueAsync(User u, string? ip, string? userAgent, CancellationToken ct)
    {
        string raw = TokenUtils.CreateSecureToken();
        RefreshToken entity = new RefreshToken
        {
            UserId = u.Id,
            TokenHash = TokenUtils.Sha256(raw),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(Lifetime),
            CreatedByIp = ip,
            UserAgent = userAgent
        };

        db.Set<RefreshToken>().Add(entity);
        await db.SaveChangesAsync(ct);
        return (entity, raw);
    }

    public async Task<(RefreshToken newEntity, string newRawToken)> RotateAsync(RefreshToken current, string? ip, string? userAgent, CancellationToken ct)
    {
        if (current.RevokedAt is not null || DateTimeOffset.UtcNow >= current.ExpiresAt)
            throw new InvalidOperationException("Refresh token inválido.");

        current.RevokedAt = DateTimeOffset.UtcNow;

        string raw = TokenUtils.CreateSecureToken();
        RefreshToken next = new RefreshToken
        {
            UserId = current.UserId,
            TokenHash = TokenUtils.Sha256(raw),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(Lifetime),
            CreatedByIp = ip,
            UserAgent = userAgent
        };
        current.ReplacedByTokenId = next.Id;

        db.Set<RefreshToken>().Add(next);
        await db.SaveChangesAsync(ct);
        return (next, raw);
    }

    public async Task RevokeFamilyAsync(RefreshToken anyTokenInFamily, CancellationToken ct)
    {
        // Estratégia simples: revogar todos os tokens ativos do usuário
        await db.Set<RefreshToken>()
            .Where(t => t.UserId == anyTokenInFamily.UserId && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);
    }
}

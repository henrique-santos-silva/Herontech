using Herontech.Domain;

namespace Herontech.Contracts.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
}

public interface IRefreshTokenService
{
    Task<(RefreshToken entity, string rawToken)> IssueAsync(User user, string? ip, string? userAgent, CancellationToken ct);
    Task<(RefreshToken newEntity, string newRawToken)> RotateAsync(RefreshToken current, string? ip, string? userAgent, CancellationToken ct);
    Task RevokeFamilyAsync(RefreshToken anyTokenInFamily, CancellationToken ct);
}
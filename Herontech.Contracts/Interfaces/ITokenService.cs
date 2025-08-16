using Herontech.Domain;

namespace Herontech.Contracts.Interfaces;

public readonly record struct AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
public interface ITokenService
{
    AccessTokenResult CreateAccessToken(User u, DateTimeOffset now);
}
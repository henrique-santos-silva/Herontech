using Herontech.Application.Security;
using Herontech.Contracts;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Herontech.Api.Routes.V1;

public static class Auth
{
    
    record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, string RefreshToken, DateTimeOffset RefreshExpiresAt);
    record RefreshRequest(string RefreshToken);
    
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/login", LoginHandler);
        endpointRouteBuilder.MapPost("/auth/refresh", RefreshHandler);
        endpointRouteBuilder.MapPost("/auth/revoke-all", RevokeAllHandler);
        
        return endpointRouteBuilder;
    }

    private static async Task<IResult> RevokeAllHandler(
        [FromBody] RefreshRequest req,
        [FromServices] AppDbContext db,
        [FromServices] IRefreshTokenService rts,
        CancellationToken ct)
    {
        byte[] hash = TokenUtils.Sha256(req.RefreshToken);
        RefreshToken? current = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (current is null) return Results.Ok();

        await rts.RevokeFamilyAsync(current, ct);
        return Results.Ok();
    }

    private static async Task<IResult> RefreshHandler(
        [FromBody] RefreshRequest req,
        [FromServices] AppDbContext db,
        [FromServices] ITokenService tokens, 
        [FromServices] IRefreshTokenService rts,
        HttpContext http, CancellationToken ct)
    {
        byte[] hash = TokenUtils.Sha256(req.RefreshToken);
        RefreshToken? current = await db.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (current is null || !current.IsActive) return Results.Unauthorized();

        (RefreshToken next, string rawNext) = await rts.RotateAsync(
            current, 
            http.Connection.RemoteIpAddress?.ToString(),
            http.Request.Headers.UserAgent.ToString(), ct);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        (string access, DateTimeOffset accessExp) = tokens.CreateAccessToken(current.User, now);

        return Results.Ok(new AuthResponse(access, accessExp, rawNext, next.ExpiresAt));
    }

    private static async Task<IResult> LoginHandler(
        [FromBody] LoginRequest req,
        [FromServices] AppDbContext db,
        [FromServices] ITokenService tokens,
        [FromServices] IRefreshTokenService rts,
        HttpContext http,
        CancellationToken ct)
    {
        User? u = await db.Users.FirstOrDefaultAsync(x => x.Email == req.Email, ct);
        
        if (u is null || !PasswordHasher.VerifyPassword(req.Password, u.PassWordSalt, u.PasswordHash)) 
            return Results.Unauthorized();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        (string access, DateTimeOffset accessExp) = tokens.CreateAccessToken(u, now);
        
        (RefreshToken rtEntity, string rawRt) = await rts.IssueAsync(
            user: u,
            ip: http.Connection.RemoteIpAddress?.ToString(),
            userAgent: http.Request.Headers.UserAgent.ToString(),
            ct);

        return Results.Ok(new AuthResponse(access, ExpiresAt: accessExp, RefreshToken: rawRt, rtEntity.ExpiresAt));
    }
}
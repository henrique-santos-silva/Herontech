using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Herontech.Api.AuthConfig;
using Herontech.Application.Security;
using Herontech.Contracts;
using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Herontech.Api.Routes.V1;

public static class Auth
{
    record RefreshRequest(string RefreshToken);
    
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/login", LoginHandler);
        endpointRouteBuilder.MapPost("/refresh", RefreshHandler);
        endpointRouteBuilder.MapPost("/revoke-all", RevokeAllHandler);
        
        return endpointRouteBuilder;
    }

    private static async Task<IResult> RevokeAllHandler(
        [FromBody] RefreshRequest req,
        [FromServices] AppDbContext db,
        [FromServices] IRefreshTokenService rts,
        CancellationToken ct)
    {
        byte[] hash = TokenUtils.Sha256(req.RefreshToken);
        RefreshToken? current = await db.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (current is null) return Results.Ok();

        await rts.RevokeFamilyAsync(current, ct);
        return Results.Ok();
    }

    private static async Task<IResult> RefreshHandler(
        [FromServices] AppDbContext db,
        [FromServices] ITokenService tokens,
        [FromServices] IRefreshTokenService rts,
        IConfiguration cfg,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.Request.Cookies.TryGetValue(AuthCookies.Refresh, out var rawRefresh) || string.IsNullOrWhiteSpace(rawRefresh))
            return Results.Unauthorized();

        byte[] hash = TokenUtils.Sha256(rawRefresh);
        RefreshToken? current = await db.Set<RefreshToken>().Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (current?.IsActive != true) return Results.Unauthorized();

        var (next, newRaw) = await rts.RotateAsync(
            current,
            http.Connection.RemoteIpAddress?.ToString(),
            http.Request.Headers.UserAgent.ToString(),
            ct);

        var now = DateTimeOffset.UtcNow;
        var (access, accessExp) = tokens.CreateAccessToken(current.User, now);

        bool prod = string.Equals(cfg["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase);

        http.Response.Cookies.Append(AuthCookies.Access, access, AuthCookies.AccessOpts(accessExp, prod));
        http.Response.Cookies.Append(AuthCookies.Refresh, newRaw, AuthCookies.RefreshOpts(next.ExpiresAt, prod));

        return Results.NoContent();
    }


    private static async Task<IResult> LoginHandler(
        [FromBody] LoginRequest req,
        [FromServices] AppDbContext db,
        [FromServices] ITokenService tokenService,
        [FromServices] IRefreshTokenService refreshTokenService,
        [FromServices] IConfiguration cfg,
        HttpContext http,
        CancellationToken ct)
    {
        User? u = await db.Set<User>().FirstOrDefaultAsync(x => x.Email == req.Email, ct);
        
        if (u is null || !PasswordHasher.VerifyPassword(req.Password, u.PassWordSalt, u.PasswordHash)) 
            return Results.Unauthorized();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        (string access, DateTimeOffset accessExp) = tokenService.CreateAccessToken(u, now);
        
        (RefreshToken rtEntity, string rawRt) = await refreshTokenService.IssueAsync(
            user: u,
            ip: http.Connection.RemoteIpAddress?.ToString(),
            userAgent: http.Request.Headers.UserAgent.ToString(),
            ct);

        bool prod = string.Equals(cfg["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase);

        http.Response.Cookies.Append(AuthCookies.Access, access, AuthCookies.AccessOpts(accessExp, prod));
        http.Response.Cookies.Append(AuthCookies.Refresh, rawRt, AuthCookies.RefreshOpts(rtEntity.ExpiresAt, prod));

        return Results.NoContent();
    }
}
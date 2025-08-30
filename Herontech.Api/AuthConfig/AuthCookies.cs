namespace Herontech.Api.AuthConfig;

public static class AuthCookies
{
    public const string Access = "acc";     // curto (ex: 15min)
    public const string Refresh = "rt";     // longo (ex: 7-30d)

    public static CookieOptions AccessOpts(DateTimeOffset expires, bool production) => new()
    {
        HttpOnly = true,
        Secure = production,
        SameSite = SameSiteMode.Lax, // API e SPA mesmo site; se cross-site, use None
        Expires = expires,
        Path = "/",
    };

    public static CookieOptions RefreshOpts(DateTimeOffset expires, bool production) => new()
    {
        HttpOnly = true,
        Secure = production,
        // Ideal: restringir o escopo de uso do refresh
        SameSite = SameSiteMode.Lax, // se front e API em domínios diferentes, use None
        Expires = expires,
        Path = "/api/v1/auth", 
    };

    public static CookieOptions Deletion(bool production) => new()
    {
        HttpOnly = true,
        Secure = production,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UnixEpoch,
        Path = "/"
    };
}
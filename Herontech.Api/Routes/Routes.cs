using Herontech.Api.Routes.V1;

namespace Herontech.Api.Routes;

public static class Routes
{
    public static WebApplication MapApi(this WebApplication app)
    {
        RouteGroupBuilder api = app.MapGroup("/api")
            .WithOpenApi()
            .RequireRateLimiting("fixed");
        
        RouteGroupBuilder v1 = api.MapGroup("/v1").WithGroupName("v1");
        
        v1.MapGroup("/auth").WithGroupName("auth").MapAuthEndpoints();
        v1.MapGroup("/users").WithGroupName("users").MapUserEndpoints();
        
        return app;
    }
}
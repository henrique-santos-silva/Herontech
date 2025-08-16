using System.Security.Claims;
using Herontech.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Herontech.Application;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid? UserId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated is not true) return null;
            string? sub = _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub is null) return null;
            return Guid.Parse(sub);
        }
    }
    
    public string? Email
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated is not true) return null;
            return _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
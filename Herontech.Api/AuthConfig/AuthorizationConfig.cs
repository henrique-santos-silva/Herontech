using Herontech.Domain;

namespace Herontech.Api.AuthConfig;

public static class AuthorizationConfig
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.SysAdminOnly, p => p.RequireRole(
                RoleEnum.SysAdmin.ToJwtString()
            ))
            .AddPolicy(AuthorizationPolicies.EmployeeOrHigher, p => p.RequireRole(
                RoleEnum.SysAdmin.ToJwtString(),
                RoleEnum.Employee.ToJwtString()
            ))
            .AddPolicy(AuthorizationPolicies.ExternalAllowed, p => p.RequireRole(
                RoleEnum.SysAdmin.ToJwtString(),
                RoleEnum.Employee.ToJwtString(),
                RoleEnum.ExternalContact.ToJwtString()
            ));

        return services;
    }

    private static string ToJwtString(this RoleEnum role) => ((int)role).ToString();
}

public static class AuthorizationPolicies
{
    public const string SysAdminOnly = nameof(SysAdminOnly);
    public const string EmployeeOrHigher = nameof(EmployeeOrHigher);
    public const string ExternalAllowed =  nameof(ExternalAllowed);
}
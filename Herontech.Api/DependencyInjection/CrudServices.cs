using System.Reflection;
using Herontech.Application.Crud;
using Herontech.Contracts.Interfaces;

namespace Herontech.Api.DependencyInjection;

public static class CrudServices
{
    public static IServiceCollection AddCrudServices(this IServiceCollection services)
    {
        var assembly = typeof(AbstractCrudService<>).Assembly; // ou typeof(ClientCrudService).Assembly
        var serviceType = typeof(ICrudService<>);

        var candidates = assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == serviceType
                )
            );

        foreach (var impl in candidates)
        {
            var implementedCrudInterfaces = impl.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType);

            foreach (var crudInterface in implementedCrudInterfaces)
            {
                services.AddScoped(crudInterface, impl);
            }
        }

        return services;
    }
}
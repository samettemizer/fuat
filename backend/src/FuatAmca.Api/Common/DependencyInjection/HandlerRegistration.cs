using System.Reflection;

namespace FuatAmca.Api.Common.DependencyInjection;

/// <summary>
/// Feature handler'larini DI'ya kaydeder. Endpointler handler'i DI'dan cozer,
/// araya mediator girmez.
/// </summary>
public static class HandlerRegistration
{
    /// <summary>Adi "Handler" ile biten tum somut siniflari scoped olarak kaydeder.</summary>
    public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                && type.Name.EndsWith("Handler", StringComparison.Ordinal))
            {
                services.AddScoped(type);
            }
        }

        return services;
    }
}

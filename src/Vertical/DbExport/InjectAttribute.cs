using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Vertical.DbExport;

[AttributeUsage(AttributeTargets.Class)]
public class InjectAttribute : Attribute
{
    public InjectAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Lifetime = lifetime;
    }

    public ServiceLifetime Lifetime { get; }
}

public static class InjectExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var injectTypes = typeof(InjectAttribute)
            .Assembly
            .GetTypes()
            .Select(type => new
            {
                type,
                injection = type.GetCustomAttribute<InjectAttribute>()
            })
            .Where(e => e.injection != null);

        foreach (var entry in injectTypes)
        {
            var descriptor = ServiceDescriptor.Describe(
                entry.type.GetInterfaces().First(),
                entry.type,
                entry.injection!.Lifetime);

            services.Add(descriptor);
        }
        
        return services;
    }
}
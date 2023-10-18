using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Vertical.DbExport.Pipeline;
using Vertical.Pipelines.DependencyInjection;

namespace Vertical.DbExport.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core export services.
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="servicesBuilder">Builder object</param>
    /// <returns>A reference to this instance.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        Action<ServicesBuilder>? servicesBuilder = null)
    {
        servicesBuilder?.Invoke(new ServicesBuilder(services));
        
        return services
            .AddPipeline()
            .AddServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }

    /// <summary>
    /// Adds core services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assembly">Assembly where the services are located.</param>
    /// <returns>A reference to this instance.</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var entries = assembly
            .GetTypes()
            .Select(type => (type, registration: type.GetCustomAttribute<ServiceRegistrationAttribute>()))
            .Where(entry => entry.registration != null);

        foreach (var entry in entries)
        {
            var interfaces = entry
                .type
                .GetInterfaces()
                .Where(type => type != typeof(IDisposable) && type != typeof(IAsyncDisposable))
                .ToArray();

            var lifetime = entry.registration!.Lifetime;

            switch (interfaces.Length)
            {
                case 0:
                    services.Add(ServiceDescriptor.Describe(
                        entry.type, 
                        entry.type,
                        lifetime));
                    break;
                
                case 1:
                    services.Add(ServiceDescriptor.Describe(
                        interfaces[0],
                        entry.type,
                        lifetime));
                    break;
                
                default:
                    services.Add(ServiceDescriptor.Describe(
                        entry.type,
                        entry.type,
                        lifetime));

                    foreach (var interfaceType in interfaces)
                    {
                        services.Add(ServiceDescriptor.Describe(
                            interfaceType,
                            provider => provider.GetRequiredService(entry.type),
                            lifetime));
                    }
                    
                    break;
            }
        }
        
        return services;
    }
}
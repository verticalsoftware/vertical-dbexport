using Microsoft.Extensions.DependencyInjection;

namespace Vertical.DbExport.Services;

/// <summary>
/// Decorates a class for service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceRegistrationAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    /// <param name="lifetime">Lifetime.</param>
    /// <param name="type">Service type</param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime, Type? type = null)
    {
        Lifetime = lifetime;
        Type = type;
    }

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public Type? Type { get; }
}
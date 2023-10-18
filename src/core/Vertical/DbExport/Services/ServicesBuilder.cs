using Microsoft.Extensions.DependencyInjection;

namespace Vertical.DbExport.Services;

/// <summary>
/// Builder class for DbExport services.
/// </summary>
public class ServicesBuilder
{
    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public ServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services { get; }
}
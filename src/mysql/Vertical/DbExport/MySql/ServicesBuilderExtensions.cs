using Vertical.DbExport.Services;

namespace Vertical.DbExport.MySql;

public static class ServicesBuilderExtensions
{
    public static ServicesBuilder AddMySql(this ServicesBuilder servicesBuilder)
    {
        servicesBuilder.Services.AddServicesFromAssembly(typeof(ServicesBuilderExtensions).Assembly);
        return servicesBuilder;
    }
}
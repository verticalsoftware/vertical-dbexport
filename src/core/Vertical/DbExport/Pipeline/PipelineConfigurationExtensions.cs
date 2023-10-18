using Microsoft.Extensions.DependencyInjection;
using Vertical.DbExport.Pipeline.Export;
using Vertical.DbExport.Pipeline.Jobs;
using Vertical.Pipelines.DependencyInjection;

namespace Vertical.DbExport.Pipeline;

public static class PipelineConfigurationExtensions
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        return services
            .ConfigurePipeline<ExportContext>(tasks => tasks
                    .UseMiddleware<ValidateOptionsTask>()
                    .UseMiddleware<ValidateDriverTask>()
                    .UseMiddleware<OrchestrateJobsTask>()
                , ServiceLifetime.Singleton)
            
            .ConfigurePipeline<JobContext>(tasks => tasks
                .UseMiddleware<QuerySchemaTask>()
                .UseMiddleware<ValidateSortColumnTask>()
                .UseMiddleware<RunPartitionedQueriesTask>()
                .UseMiddleware<FlushDataFilesTask>());
    }
}
using FluentValidation;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline;

public class ValidateConfigurationTask : IPipelineMiddleware<RootOptions>
{
    private readonly ILogger<ValidateConfigurationTask> _logger;
    private readonly IValidator<RootOptions> _validator;

    public ValidateConfigurationTask(
        ILogger<ValidateConfigurationTask> logger,
        IValidator<RootOptions> validator)
    {
        _logger = logger;
        _validator = validator;
    }
    
    public async Task InvokeAsync(RootOptions context, 
        PipelineDelegate<RootOptions> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating configuration");
        var result = await _validator.ValidateAsync(context, cancellationToken);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogError("Validation error: {message}", error.ErrorMessage);
            }
            _logger.LogError("Validation failed");
            return;
        }
        
        _logger.LogInformation("Configuration validated");

        await next(context, cancellationToken);
    }
}
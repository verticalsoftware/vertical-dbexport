using FluentValidation;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Export;

public class ValidateOptionsTask : IPipelineMiddleware<ExportContext>
{
    private readonly ILogger<ValidateOptionsTask> _logger;
    private readonly IValidator<ExportOptions> _validator;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public ValidateOptionsTask(ILogger<ValidateOptionsTask> logger, IValidator<ExportOptions> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(
        ExportContext context,
        PipelineDelegate<ExportContext> next,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        var result = _validator.Validate(context.Options);

        if (result.IsValid)
        {
            _logger.LogInformation("Configuration passed initial validation.");
            await next(context, cancellationToken);
            return;
        }

        foreach (var error in result.Errors)
        {
            _logger.LogError("Validation error:\n\tProperty: {property}\n\tMessage:  {message}\n\tValue:    '{value}',",
                error.PropertyName,
                error.ErrorMessage,
                error.AttemptedValue);
        }

        throw new ApplicationException("Validation failed");
    }
}
using FluentValidation;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Validators;

public class TaskOptionsValidator : AbstractValidator<TaskOptions>
{
    public TaskOptionsValidator(
        RootOptions rootOptions,
        JobOptions job)
    {
        RuleFor(x => x.Connection)
            .NotEmpty()
            .WithMessage((x, _) => FormatMessage(job, x, "Missing connection key."));

        RuleFor(x => x.Connection)
            .Must(v => rootOptions.Connections.ContainsKey(v))
            .WithMessage((x, v) => FormatMessage(job, x, $"Invalid connection key '{v}'."));

        RuleFor(x => x.BatchCount)
            .GreaterThan(0)
            .WithMessage((x, v) => FormatMessage(job, x, "BatchCount must be greater than zero."));

        RuleFor(x => x.Table)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Query))
            .WithMessage((x, v) => FormatMessage(job, x, "Table name required when Query is not specified."));

        RuleFor(x => x.Query)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Table))
            .WithMessage((x, v) => FormatMessage(job, x, "Query required when Table is not specified."));
        
        RuleFor(x => x.Schema)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Query))
            .WithMessage((x, v) => FormatMessage(job, x, "Schema required when Query is not specified."));

        RuleFor(x => x.Output).SetValidator(new OutputOptionsValidator());

        RuleFor(x => x.Output).NotNull().WithMessage((x, v) => FormatMessage(job, x, "Output options not defined."));

        RuleFor(x => x.CommandRetries)
            .GreaterThan(0)
            .When(options => options.CommandRetries.HasValue)
            .WithMessage((x, v) => FormatMessage(job, x, $"Command retries value '{v}' invalid."));

        RuleFor(x => x.TaskId).NotEmpty();
    }

    private static string FormatMessage(JobOptions job, TaskOptions task, string msg)
    {
        return $"Job[{job.JobId}].Tasks[{task.TaskId}]: {msg}";
    }
}
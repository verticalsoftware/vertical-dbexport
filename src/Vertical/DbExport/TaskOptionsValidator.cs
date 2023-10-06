using FluentValidation;

namespace Vertical.DbExport;

public class TaskOptionsValidator : AbstractValidator<TaskOptions>
{
    public TaskOptionsValidator(
        RootOptions rootOptions,
        JobOptions job,
        IConnectionProvider connectionProvider)
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

        RuleFor(x => x.Output).SetValidator(new OutputOptionsValidator());
    }

    private static string FormatMessage(JobOptions job, TaskOptions task, string msg)
    {
        return $"Job[{job.JobId}].Tasks[{task.TaskId}]: {msg}";
    }
}
using FluentValidation;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Validators;

public class JobOptionsValidator : AbstractValidator<JobOptions>
{
    public JobOptionsValidator(RootOptions rootOptions)
    {
        RuleFor(x => x.JobId).NotEmpty();

        RuleForEach(x => x.Tasks).SetValidator((job, _) => new TaskOptionsValidator(rootOptions, job));
    }
}
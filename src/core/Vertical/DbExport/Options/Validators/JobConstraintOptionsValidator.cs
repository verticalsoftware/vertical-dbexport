using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class JobConstraintOptionsValidator : AbstractValidator<JobConstraintOptions>
{
    public JobConstraintOptionsValidator()
    {
        RuleFor(x => x.MaxRowCount).GreaterThan(0);
    }
}
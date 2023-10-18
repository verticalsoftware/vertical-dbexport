using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class JobOptionsValidator : AbstractValidator<JobOptions>
{
    public JobOptionsValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.DataSource).NotNull().SetValidator(new DataSourceOptionsValidator());
        RuleFor(x => x.Output).NotNull().SetValidator(new OutputOptionsValidator());
        RuleFor(x => x.Parallelization).NotNull().SetValidator(new ParallelizationOptionsValidator());
        RuleFor(x => x.Constraints).NotNull().SetValidator(new JobConstraintOptionsValidator());
    }
}
using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class ParallelizationOptionsValidator : AbstractValidator<ParallelizationOptions>
{
    public ParallelizationOptionsValidator()
    {
        RuleFor(x => x.MaxPartitionThreads).GreaterThan(0);
        RuleFor(x => x.QueryBatchSize).GreaterThan(1);
        RuleFor(x => x.PartitionSize)
            .GreaterThan(1)
            .Must((x, i) => i >= x.QueryBatchSize)
            .WithMessage("Partition size must be equal to or greater than batch size.");
    }
}
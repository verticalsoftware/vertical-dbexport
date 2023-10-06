using FluentValidation;

namespace Vertical.DbExport;

public class JobOptionsValidator : AbstractValidator<JobOptions>
{
    public JobOptionsValidator(RootOptions rootOptions)
    {
        
    }
}
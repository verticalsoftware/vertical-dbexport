using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.Options.Validators;

[ServiceRegistration(ServiceLifetime.Singleton, typeof(IValidator<ExportOptions>))]
public class ExportOptionsValidator : AbstractValidator<ExportOptions>
{
    public ExportOptionsValidator()
    {
        RuleFor(x => x.Connection).NotNull().SetValidator(new ConnectionOptionsValidator());
        RuleForEach(x => x.Jobs).SetValidator(new JobOptionsValidator());
        RuleFor(x => x.Jobs)
            .Must(jobs => jobs.DistinctBy(j => j.Name).Count() == jobs.Length)
            .WithMessage("Job names must be unique");
    }
}
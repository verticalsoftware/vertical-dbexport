using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class ConnectionOptionsValidator : AbstractValidator<ConnectionOptions>
{
    public ConnectionOptionsValidator()
    {
        RuleFor(x => x.ConnectionString).NotNull().NotEmpty();
        RuleFor(x => x.Driver).NotNull().NotEmpty();
        RuleFor(x => x.ConnectionRetryCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommandRetryCount).GreaterThanOrEqualTo(0);
    }
}
using FluentValidation;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Validators;

public class ConnectionOptionsValidator : AbstractValidator<KeyValuePair<string, ConnectionOptions>>
{
    public ConnectionOptionsValidator(IDatabaseProviderFactory providerFactory)
    {
        RuleFor(x => x.Value.ProviderId)
            .Cascade(CascadeMode.Stop)
            .Must(v => providerFactory.GetProvider(v) != null)
            .WithMessage(v => $"Connections[{v.Key}]: unsupported provider '{v.Value.ProviderId}'");
    }
}
using FluentValidation;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Validators;

public class RootOptionsValidator : AbstractValidator<RootOptions>
{
    public RootOptionsValidator(IDatabaseProviderFactory providerFactory)
    {
        RuleFor(x => x.Connections)
            .ForEach(entry => entry
                .Must(kv => providerFactory.GetProvider(kv.Value.ProviderId) != null)
                .WithMessage((_, kv) => $"Connections[{kv.Key}]: unsupported provider '{kv.Value.ProviderId}'"));

        RuleFor(x => x.Connections)
            .ForEach(entry => entry
                .MustAsync(async (kv, _) =>
                {
                    var provider = providerFactory.GetProvider(kv.Value.ProviderId);
                    return provider == null || await provider.TestConnectionAsync(kv.Value);
                })
                .WithMessage((_, kv) => $"Connections[{kv.Key}]: connection test failed."));

        RuleForEach(x => x.Connections).SetValidator(new ConnectionOptionsValidator(providerFactory));
    }
}
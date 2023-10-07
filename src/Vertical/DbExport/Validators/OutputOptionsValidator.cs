using FluentValidation;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Validators;

public class OutputOptionsValidator : AbstractValidator<OutputOptions>
{
    public OutputOptionsValidator()
    {
        RuleFor(x => x.Compression).Must(value => value is "none" or "gzip");
        
        RuleFor(x => x.Format).Equal("json");
        
        RuleFor(x => x.PathTemplate).NotEmpty();

        RuleFor(x => x.MaxUncompressedSize)
            .Matches(@"^(\.?\d+\.?\d+)([BMG])$")
            .When(x => !string.IsNullOrWhiteSpace(x.MaxUncompressedSize))
            .WithMessage(v => $"invalid MaxUncompressedSize '{v.MaxUncompressedSize}'.");
    }
}
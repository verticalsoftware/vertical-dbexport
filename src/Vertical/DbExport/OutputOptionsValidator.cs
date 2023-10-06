using FluentValidation;

namespace Vertical.DbExport;

public class OutputOptionsValidator : AbstractValidator<OutputOptions>
{
    public OutputOptionsValidator()
    {
        RuleFor(x => x.Compression).Must(value => value is "none" or "gzip");
        RuleFor(x => x.Format).Equal("json");
        RuleFor(x => x.PathTemplate).NotEmpty();
    }
}
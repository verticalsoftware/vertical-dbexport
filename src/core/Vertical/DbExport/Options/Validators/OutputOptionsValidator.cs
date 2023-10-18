using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class OutputOptionsValidator : AbstractValidator<OutputOptions>
{
    public OutputOptionsValidator()
    {
        RuleFor(x => x.FileSize)
            .Must(value => OutputOptions.FileTemplateExpression.IsMatch(value))
            .WithMessage(value => $"Invalid file size specification '{value}'");
    }
}
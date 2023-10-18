using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class ColumnDefinitionValidator : AbstractValidator<ColumnDefinition?>
{
    public ColumnDefinitionValidator()
    {
        RuleFor(x => x!.ColumnName).NotNull().NotEmpty();
    }
}
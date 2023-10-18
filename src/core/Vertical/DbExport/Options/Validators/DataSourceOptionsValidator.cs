using FluentValidation;

namespace Vertical.DbExport.Options.Validators;

public class DataSourceOptionsValidator : AbstractValidator<DataSourceOptions>
{
    public DataSourceOptionsValidator()
    {
        RuleFor(x => x.TableName).NotNull().NotEmpty();
        RuleFor(x => x.SortColumn).SetValidator(new ColumnDefinitionValidator());
    }
}
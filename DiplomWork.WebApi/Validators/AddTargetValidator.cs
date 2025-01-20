using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class AddBudgetValidator : AbstractValidator<AddBudgetDTO>
    {
        public AddBudgetValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.Limit).GreaterThan(0);
            RuleFor(x => x.PeriodType).GreaterThanOrEqualTo(0).LessThan(5);
        }
    }
}

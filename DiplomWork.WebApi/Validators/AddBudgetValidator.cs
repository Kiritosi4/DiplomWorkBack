using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class AddBudgetValidator : AbstractValidator<AddBudgetDTO>
    {
        const byte MAX_MANTISS = 2;
        public AddBudgetValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().Length(1, 32);
            RuleFor(x => x.Limit).GreaterThan(0).LessThanOrEqualTo(100000000000000);
            RuleFor(x => x.PeriodType).GreaterThanOrEqualTo(0).LessThan(5);
            RuleFor(x => x.Limit.Scale).LessThanOrEqualTo(MAX_MANTISS);

            RuleFor(x => x.EndPeriod).GreaterThanOrEqualTo(x => x.StartPeriod);
        }
    }
}

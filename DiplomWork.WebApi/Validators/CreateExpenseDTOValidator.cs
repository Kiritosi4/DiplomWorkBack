using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class CreateExpenseDTOValidator : AbstractValidator<CreateExpenseDTO>
    {
        const byte MAX_MANTISS = 2;
        public CreateExpenseDTOValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000000000);
            RuleFor(x => x.Timestamp).GreaterThan(0);
            RuleFor(x => x.Amount.Scale).LessThanOrEqualTo(MAX_MANTISS);
        }
    }
}

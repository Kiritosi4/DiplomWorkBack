using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class CreateExpenseDTOValidator : AbstractValidator<CreateExpenseDTO>
    {
        public CreateExpenseDTOValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Timestamp).GreaterThan(0);
        }
    }
}

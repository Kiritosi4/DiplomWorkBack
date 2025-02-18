using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class AddTargetValidator : AbstractValidator<AddTargetDTO>
    {
        public AddTargetValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().Length(1, 32);
            RuleFor(x => x.Limit).GreaterThan(0);
        }
    }
}

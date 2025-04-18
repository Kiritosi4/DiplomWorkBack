using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class AddTargetValidator : AbstractValidator<AddTargetDTO>
    {
        const byte MAX_MANTISS = 2;
        public AddTargetValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().Length(1, 32);

            RuleFor(x => x.Limit).GreaterThan(0).LessThanOrEqualTo(100000000000000);
            RuleFor(x => x.Limit.Scale).LessThanOrEqualTo(MAX_MANTISS);

            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(100000000000000);
            RuleFor(x => x.Amount.Scale).LessThanOrEqualTo(MAX_MANTISS);
        }
    }
}

using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class CreateProfitDTOValidator : AbstractValidator<CreateProfitDTO>
    {
        public CreateProfitDTOValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Timestamp).GreaterThan(0);
        }
    }
}

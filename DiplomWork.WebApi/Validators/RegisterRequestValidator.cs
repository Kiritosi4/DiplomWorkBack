using DiplomWork.WebApi.Contracts;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestBody>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}

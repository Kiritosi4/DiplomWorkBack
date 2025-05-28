using DiplomWork.WebApi.Contracts;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestBody>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(128);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}

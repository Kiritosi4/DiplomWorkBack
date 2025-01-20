using DiplomWork.DTO;
using FluentValidation;

namespace DiplomWork.WebApi.Validators
{
    public class AddCategoryValidator : AbstractValidator<AddCategoryDTO>
    {
        public AddCategoryValidator()
        {
            RuleFor(x => x.Name).NotNull().Length(1, 128);
        }
    }
}

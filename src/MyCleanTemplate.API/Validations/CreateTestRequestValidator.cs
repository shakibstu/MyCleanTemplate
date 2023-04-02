using FluentValidation;
using MyCleanTemplate.API.Models;

namespace MyCleanTemplate.API.Validations
{
    public class CreateTestRequestValidator : AbstractValidator<CreateTestRequest>
    {
        public CreateTestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).MinimumLength(3);
            RuleFor(x => x.Details).SetValidator(new CreateTestDetailsValidator());
        }
    }
    public class CreateTestDetailsValidator : AbstractValidator<CreateTestDetails>
    {
        public CreateTestDetailsValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
